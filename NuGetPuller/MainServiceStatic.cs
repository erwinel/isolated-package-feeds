using Microsoft.Extensions.Logging;
using IsolatedPackageFeeds.Shared;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Newtonsoft.Json;
using static IsolatedPackageFeeds.Shared.CommonStatic;
using static NuGetPuller.NuGetPullerStatic;

namespace NuGetPuller;

public partial class MainServiceStatic
{
    public static OfflinePackageManifest[] LoadOfflinePackageManifest(string? path, ILogger logger)
    {
        if (string.IsNullOrEmpty(path))
            return [];
        using StreamReader reader = OpenStreamReader(path,
            (p, e) => logger.PackageMetadataFileAccessDenied(path, m => new OfflineMetaDataIOException(p, m, e), e),
            (p, e) => logger.PackageManifestOpenError(path, m => new OfflineMetaDataIOException(p, m, e), e));
        try
        {
            using JsonTextReader jsonReader = new(reader);
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(MetadataDeSerializationSettings);        
            return jsonSerializer.Deserialize<OfflinePackageManifest[]>(jsonReader) ?? [];
        }
        catch (JsonReaderException exception)
        {
            throw logger.PackageMetadataFileReadError(path, m => new OfflineMetaDataIOException(path, m, exception), exception);
        }
    }

    private static FileInfo GetExportBundleFileInfos(string bundlePath, string? targetMetadataInput, string? targetMetaDataOutput, ILogger logger, CancellationToken cancellationToken,
        out FileInfo targetMetadataFileInfo, out HashSet<OfflinePackageManifest> existingPackages)
    {
        var bundleFileInfo = GetFileInfo(bundlePath,
            (p, e) => logger.ExportBundleAccessError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
            (p, e) => logger.InvalidExportBundle(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
            p => logger.ExportBundlePathNotAFile(p, m => new OfflineMetaDataIOException(p, m)),
            p => logger.ExportBundleDirectoryNotFound(p, m => new OfflineMetaDataIOException(p, m)));
        if (string.IsNullOrEmpty(targetMetadataInput))
        {
            if (string.IsNullOrWhiteSpace(targetMetaDataOutput))
            {
                if (bundleFileInfo.Directory is null)
                    throw logger.InvalidExportBundle(bundlePath, m => new OfflineMetaDataIOException(bundleFileInfo.FullName, m));
                cancellationToken.ThrowIfCancellationRequested();
                targetMetadataFileInfo = GetUniqueFileInfo(bundleFileInfo.Directory, Path.GetFileNameWithoutExtension(bundleFileInfo.Name), ".json",
                    (p, e) => logger.ExportBundleAccessError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    (p, e) => logger.InvalidExportBundle(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e));
            }
            else
                targetMetadataFileInfo = GetFileInfo(targetMetaDataOutput,
                    (p, e) => logger.PackageMetadataFileAccessDenied(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    (p, e) => logger.PackageManifestOpenError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    p => logger.LocalPackageManifestNotAFile(p, m => new OfflineMetaDataIOException(p, m)),
                    p => logger.LocalPackageManifestDirectoryNotFound(p, m => new OfflineMetaDataIOException(p, m)));
            existingPackages = [];
        }
        else
        {
            var fileInfo = GetExistingFileInfo(targetMetadataInput,
                (p, e) => logger.PackageMetadataFileAccessDenied(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                (p, e) => logger.PackageManifestOpenError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                p => logger.LocalPackageManifestNotAFile(p, m => new OfflineMetaDataIOException(p, m)),
                p => logger.LocalPackageManifestDirectoryNotFound(p, m => new OfflineMetaDataIOException(p, m)));
            cancellationToken.ThrowIfCancellationRequested();
            existingPackages = new(LoadOfflinePackageManifest(fileInfo.FullName, logger));
            if (string.IsNullOrWhiteSpace(targetMetaDataOutput))
                targetMetadataFileInfo = fileInfo;
            else
                targetMetadataFileInfo = GetFileInfo(targetMetaDataOutput,
                    (p, e) => logger.PackageMetadataFileAccessDenied(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    (p, e) => logger.PackageManifestOpenError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    p => logger.LocalPackageManifestNotAFile(p, m => new OfflineMetaDataIOException(p, m)),
                    p => logger.LocalPackageManifestDirectoryNotFound(p, m => new OfflineMetaDataIOException(p, m)));
        }
        return bundleFileInfo;
    }

    public static async Task ExportBundleAsync(string bundlePath, string? targetMetadataInput, string? targetMetaDataOutput, ILocalNuGetFeedService localClientService, ILogger logger, CancellationToken cancellationToken)
    {
        var bundleFileInfo = GetExportBundleFileInfos(bundlePath, targetMetadataInput, targetMetaDataOutput, logger, cancellationToken, out FileInfo targetMetadataFileInfo,
            out HashSet<OfflinePackageManifest> existingPackages);
        using TempStagingFolder tempStagingFolder = new();
        var pathResolver = new VersionFolderPathResolver(tempStagingFolder.Directory.FullName);
        await foreach (var identity in existingPackages.ConcatAsync(localClientService.GetAllPackagesAsync(cancellationToken)))
        {
            var fileName = pathResolver.GetPackageFileName(identity.Id, identity.HasVersion ? identity.Version : null);
            var fullName = Path.Combine(tempStagingFolder.Directory.FullName, fileName);
            using var stream = OpenFileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None,
                    (p, e) => logger.PackageExportAccessDenied(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e),
                    (p, e) => logger.PackageExportWriteError(bundlePath, m => new OfflineMetaDataIOException(p, m, e), e));
            await localClientService.CopyNupkgToStreamAsync(identity.Id, identity.Version, stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        await Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync(string bundlePath, string?, string?, ILocalNuGetFeedService, ILogger, CancellationToken) not implemented"));
    }

    public static async Task ExportBundleAsync(string bundlePath, string? targetMetadataInput, string? targetMetaDataOutput, string[] packageIds, ILocalNuGetFeedService localClientService, ILogger logger, CancellationToken cancellationToken)
    {
        var bundleFileInfo = GetExportBundleFileInfos(bundlePath, targetMetadataInput, targetMetaDataOutput, logger, cancellationToken, out FileInfo targetMetadataFileInfo,
            out HashSet<OfflinePackageManifest> existingPackages);
        await Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync(string bundlePath, string?, string?, string[], ILocalNuGetFeedService, ILogger, CancellationToken) not implemented"));
    }

    public static async Task ExportBundleAsync(string bundlePath, string? targetMetadataInput, string? targetMetaDataOutput, string[] packageIds, NuGetVersion[] versions, ILocalNuGetFeedService localClientService, ILogger logger, CancellationToken cancellationToken)
    {
        var bundleFileInfo = GetExportBundleFileInfos(bundlePath, targetMetadataInput, targetMetaDataOutput, logger, cancellationToken, out FileInfo targetMetadataFileInfo,
            out HashSet<OfflinePackageManifest> existingPackages);
        await Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync(string bundlePath, string?, string?, string[], NuGetVersion[], ILocalNuGetFeedService, ILogger, CancellationToken) not implemented"));
    }

    public static async Task ExportLocalManifestAsync(IEnumerable<IPackageSearchMetadata> packages, string exportPath, ILogger logger, CancellationToken cancellationToken)
    {
        using var writer = OpenStreamWriter(exportPath,
            (p, e) => logger.MetaDataExportPathAccessDenied(p, m => new OfflineMetaDataIOException(p, m, e), e),
            (p, e) => logger.InvalidLocalMetaDataExportPath(p, m => new OfflineMetaDataIOException(p, m, e), e));
        await OfflinePackageManifestConverter.ExportLocalManifestAsync(packages, writer, cancellationToken);
        await writer.FlushAsync(cancellationToken);
        writer.Close();
    }

    public static async Task AddToLocalFromRemote(string packageId, Dictionary<string, HashSet<NuGetVersion>> packagesAdded, ILocalNuGetFeedService localClientService, IUpstreamNuGetClientService upstreamClientService, ILogger logger, CancellationToken cancellationToken)
    {
        var upstreamVersions = await localClientService.GetAllVersionsAsync(packageId, cancellationToken);
        if (upstreamVersions is not null && upstreamVersions.Any())
        {
            logger.NuGetPackageAlreadyAdded(packageId);
            return;
        }
        if ((upstreamVersions = await upstreamClientService.GetAllVersionsAsync(packageId, cancellationToken)) is null || !upstreamVersions.Any())
        {
            logger.NuGetPackageNotFound(packageId, upstreamClientService);
            return;
        }
        if (packagesAdded.TryGetValue(packageId, out HashSet<NuGetVersion>? versionsAdded))
        {
            if (!(upstreamVersions = upstreamVersions.Where(v => !versionsAdded.Contains(v))).Any())
                return;
            foreach (NuGetVersion v in upstreamVersions)
                versionsAdded.Add(v);
        }
        else
        {
            versionsAdded = new(upstreamVersions, VersionComparer.VersionReleaseMetadata);
            packagesAdded.Add(packageId, versionsAdded);
        }
        using TempStagingFolder tempStagingFolder = new();
        var pathResolver = new VersionFolderPathResolver(tempStagingFolder.Directory.FullName);
        foreach (NuGetVersion v in upstreamVersions)
        {
            FileInfo packageFile;
            try
            {
                packageFile = await tempStagingFolder.NewFileInfoAsync(pathResolver.GetPackageFileName(packageId, v), async (stream, token) =>
                {
                    await upstreamClientService.CopyNupkgToStreamAsync(packageId, v, stream, token);
                }, cancellationToken);
            }
            catch (Exception error)
            {
                logger.UnexpectedPackageDownloadFailure(packageId, v, error);
                continue;
            }
            if (packageFile.Length > 0)
                await localClientService.AddPackageAsync(packageFile.FullName, false, cancellationToken);
            else
                logger.DownloadPackageIsEmpty(packageId, v);
        }
    }

    public static Task CheckDependenciesAsync(ILocalNuGetFeedService localClientService, ILogger logger, string[] packageIds, NuGetVersion[] versions, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.CheckDependenciesAsync(ILocalNuGetFeedService, ILogger, string[], NuGetVersion[], CancellationToken) not implemented"));
    }

    public static Task CheckDependenciesAsync(ILocalNuGetFeedService localClientService, ILogger logger, string[] packageIds, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync(ILocalNuGetFeedService, ILogger, string[], CancellationToken) not implemented"));
    }

    public static Task CheckAllDependenciesAsync(ILocalNuGetFeedService localClientService, ILogger logger, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.CheckAllDependenciesAsync not implemented"));
    }

    public static Task DownloadMissingDependenciesAsync(ILocalNuGetFeedService localClientService, IUpstreamNuGetClientService upstreamClientService, ILogger logger, string[] packageIds, NuGetVersion[] nuGetVersions, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync not implemented"));
    }

    public static Task DownloadMissingDependenciesAsync(ILocalNuGetFeedService localClientService, IUpstreamNuGetClientService upstreamClientService, ILogger logger, string[] packageIds, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync not implemented"));
    }

    public static Task DownloadAllMissingDependenciesAsync(ILocalNuGetFeedService localClientService, IUpstreamNuGetClientService upstreamClientService, ILogger logger, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync not implemented"));
    }

    public Task DeletePackagesAsync(ILocalNuGetFeedService localClientService, ILogger logger, string[] packageIds, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync not implemented"));
    }

    public Task DeletePackageVersionsAsync(ILocalNuGetFeedService localClientService, ILogger logger, string[] packageIds, NuGetVersion[] nuGetVersions, CancellationToken cancellationToken)
    {
        return Task.FromException(new NotImplementedException("NuGetPuller.MainServiceStatic.ExportBundleAsync not implemented"));
    }
}
