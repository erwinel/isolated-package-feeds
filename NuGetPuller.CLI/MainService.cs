using IsolatedPackageFeeds.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using static IsolatedPackageFeeds.Shared.CommonStatic;
using static NuGetPuller.MainServiceStatic;

namespace NuGetPuller.CLI;

public class MainService : BackgroundService
{
    private readonly AppSettings _settings;
    private readonly ILogger<MainService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MainService(IOptions<AppSettings> options, ILogger<MainService> logger, IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime) =>
        (_settings, _logger, _serviceProvider, _applicationLifetime) = (options.Value, logger, serviceProvider, applicationLifetime);

    private async Task WriteLocalPackagesToConsole(ILocalClientService localClientService, bool includeVersions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.IncludeVersions, () => (_logger, nameof(AppSettings.IncludeVersions), nameof(AppSettings.ListLocal)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.Version, () => (_logger, nameof(AppSettings.Version), nameof(AppSettings.Download), nameof(AppSettings.Remove), nameof(AppSettings.CheckDependencies), nameof(AppSettings.CreateBundle)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.NoDependencies, () => (_logger, nameof(AppSettings.NoDependencies), nameof(AppSettings.Download)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.SaveTo, () => (_logger, nameof(AppSettings.SaveTo), nameof(AppSettings.Remove)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.PackageId, () => (_logger, nameof(AppSettings.PackageId), nameof(AppSettings.CheckDependencies), nameof(AppSettings.CreateBundle)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.NoDownload, () => (_logger, nameof(AppSettings.NoDownload), nameof(AppSettings.CheckDependencies)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.CreateFrom, () => (_logger, nameof(AppSettings.CreateFrom), nameof(AppSettings.CreateBundle)));
        // LogIgnoredDependentCommandLineArgumentIfSet(_settings.SaveMetaDataTo, () => (_logger, nameof(AppSettings.SaveMetaDataTo), nameof(AppSettings.CreateBundle)));
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var validatedSettings = scope.ServiceProvider.GetRequiredService<ValidatedPathsService>();
            if (_settings.CheckDependencies)
            {
                if (_settings.ListLocal)
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.ListLocal));
                else if (!string.IsNullOrWhiteSpace(_settings.CreateBundle))
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.CreateBundle));
                else if (!string.IsNullOrWhiteSpace(_settings.Download))
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.Download));
                else if (!string.IsNullOrWhiteSpace(_settings.Remove))
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.Remove));
                else if (!string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.AddPackageFiles));
                else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CheckDependencies), nameof(AppSettings.ExportMetaData));
                else
                {
                    var localService = _serviceProvider.GetRequiredService<ILocalClientService>();
                    if (_settings.NoDownload)
                    {
                        if (_settings.PackageId.TrySplitToNonWhiteSpaceTrimmed(',', out string[]? packageIds))
                        {
                            if (_settings.Version.TryGetNuGetVersionList(out NuGetVersion[]? versions))
                                await CheckDependenciesAsync(localService, _logger, packageIds, versions, stoppingToken);
                            else
                                await CheckDependenciesAsync(localService, _logger, packageIds, stoppingToken);
                        }
                        else
                        {
                            LogIgnoredDependentCommandLineArgumentIfSet(_settings.Version, () => (_logger, nameof(AppSettings.Version), nameof(AppSettings.PackageId)));
                            await CheckAllDependenciesAsync(localService, _logger, stoppingToken);
                        }
                    }
                    else
                    {
                        var upstreamService = _serviceProvider.GetRequiredService<IUpstreamClientService>();
                        if (_settings.PackageId.TrySplitToNonWhiteSpaceTrimmed(',', out string[]? packageIds))
                        {
                            if (_settings.Version.TryGetNuGetVersionList(out NuGetVersion[]? versions))
                                await DownloadMissingDependenciesAsync(localService, upstreamService, _logger, packageIds, versions, stoppingToken);
                            else
                                await DownloadMissingDependenciesAsync(localService, upstreamService, _logger, packageIds, stoppingToken);
                        }
                        else
                        {
                            LogIgnoredDependentCommandLineArgumentIfSet(_settings.Version, () => (_logger, nameof(AppSettings.Version), nameof(AppSettings.PackageId)));
                            await DownloadAllMissingDependenciesAsync(localService, upstreamService, _logger, stoppingToken);
                        }
                    }
                }
            }
            else
            {
                if (_settings.ListLocal)
                {
                    if (!string.IsNullOrWhiteSpace(_settings.CreateBundle))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.ListLocal), nameof(AppSettings.CreateBundle));
                    else if (!string.IsNullOrWhiteSpace(_settings.Download))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.ListLocal), nameof(AppSettings.Download));
                    else if (!string.IsNullOrWhiteSpace(_settings.Remove))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.ListLocal), nameof(AppSettings.Remove));
                    else if (!string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.ListLocal), nameof(AppSettings.AddPackageFiles));
                    else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.ListLocal), nameof(AppSettings.ExportMetaData));
                    else
                    {
                        // ListLocal
                        // _settings.IncludeVersions
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(_settings.CreateBundle))
                    {
                        if (string.IsNullOrWhiteSpace(_settings.Download))
                        {
                            if (string.IsNullOrWhiteSpace(_settings.Remove))
                            {
                                if (string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                                {
                                    if (string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                                    {
                                        // nothing!
                                    }
                                    else
                                    {
                                        // ExportMetaData
                                    }
                                }
                                else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                                    _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.AddPackageFiles), nameof(AppSettings.ExportMetaData));
                                else
                                {
                                    // AddPackageFiles
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                                _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.Remove), nameof(AppSettings.AddPackageFiles));
                            else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                                _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.Remove), nameof(AppSettings.ExportMetaData));
                            else
                            {
                                // Remove
                                // _settings.Version, _settings.SaveTo
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(_settings.Remove))
                            _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.Download), nameof(AppSettings.Remove));
                        else if (!string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                            _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.Download), nameof(AppSettings.AddPackageFiles));
                        else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                            _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.Download), nameof(AppSettings.ExportMetaData));
                        else
                        {
                            // Download
                            // _settings.Version, _settings.NoDependencies
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(_settings.Download))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CreateBundle), nameof(AppSettings.Download));
                    else if (!string.IsNullOrWhiteSpace(_settings.Remove))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CreateBundle), nameof(AppSettings.Remove));
                    else if (!string.IsNullOrWhiteSpace(_settings.AddPackageFiles))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CreateBundle), nameof(AppSettings.AddPackageFiles));
                    else if (!string.IsNullOrWhiteSpace(_settings.ExportMetaData))
                        _logger.CommandLineArgumentsAreExclusive(nameof(AppSettings.CreateBundle), nameof(AppSettings.ExportMetaData));
                    else
                    {
                        // CreateBundle
                        // _settings.CreateFrom, _settings.SaveMetaDataTo, _settings.PackageId, _settings.Version
                    }
                }
            }

            // var packageIds = _settings.Remove?.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).Distinct(NoCaseComparer);
            // HashSet<PackageIdentity> deletedPackages = new(PackageIdentityComparer.Default);
            // if (packageIds is not null && packageIds.Any())
            //     await foreach (var (package, success) in scope.ServiceProvider.GetRequiredService<ILocalClientService>().DeleteAsync(packageIds, stoppingToken))
            //         if (success)
            //             deletedPackages.Add(package);
            // if ((packageIds = _settings.Add?.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).Distinct(NoCaseComparer)) is not null && packageIds.Any())
            // {
            //     Dictionary<string, HashSet<NuGetVersion>> packagesAdded = new(NoCaseComparer);
            //     var localClientService = scope.ServiceProvider.GetRequiredService<ILocalClientService>();
            //     foreach (string id in packageIds)
            //         await AddToLocalFromRemote(id, packagesAdded, localClientService, scope.ServiceProvider.GetRequiredService<IUpstreamClientService>(), _logger, stoppingToken);
            // }
            // if (validatedSettings.Import.TryGetResult(out FileSystemInfo? importFrom))
            //     throw new NotImplementedException();
            // // await ImportAsync(validatedSettings.ImportPath, localClientService, _logger, stoppingToken);
            // if (_settings.UpdateAll)
            // {
            //     var asyncEn = scope.ServiceProvider.GetRequiredService<ILocalClientService>().GetAllPackagesAsync(stoppingToken);
            //     if (!(packageIds = asyncEn.ToBlockingEnumerable(stoppingToken).Select(p => p.Identity.Id)).Any())
            //         _logger.NoLocalPackagesExist();
            //     else
            //         await scope.ServiceProvider.GetRequiredService<PackageUpdateService>().UpdatePackagesAsync(packageIds, stoppingToken);
            // }
            // else if ((packageIds = _settings.Update?.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).Distinct(NoCaseComparer)) is not null && packageIds.Any())
            //     await scope.ServiceProvider.GetRequiredService<PackageUpdateService>().UpdatePackagesAsync(packageIds, stoppingToken);
            // if (_settings.ListLocal)
            //     await ListLocalPackagesAsync(scope.ServiceProvider.GetRequiredService<ILocalClientService>().GetAllPackagesAsync(stoppingToken).ToBlockingEnumerable(stoppingToken), validatedSettings.ExportLocalManifest.TryGetResult(out FileInfo? exportLocalManifest) ? exportLocalManifest.FullName : null, _logger, stoppingToken);
            // if (validatedSettings.ExportLocalManifest.TryGetResult(out FileInfo? fileInfo))
            //     await ExportLocalManifestAsync(scope.ServiceProvider.GetRequiredService<ILocalClientService>().GetAllPackagesAsync(stoppingToken).ToBlockingEnumerable(stoppingToken), fileInfo.FullName, _logger, stoppingToken);
            // if (validatedSettings.ExportBundle.TryGetResult(out fileInfo))
            //     throw new NotImplementedException();
            // // await ExportBundleAsync(validatedSettings.ExportBundlePath, validatedSettings.TargetManifestFilePath, validatedSettings.TargetManifestSaveAsPath, localClientService, _logger, stoppingToken);
        }
        catch (OperationCanceledException) { throw; }
        finally
        {
            if (!stoppingToken.IsCancellationRequested)
                _applicationLifetime.StopApplication();
        }
    }
}
