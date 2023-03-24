namespace CdnGetter.Config;

/// <summary>
/// Top-level section for custom app settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Default name of database file.
    /// </summary>
    public const string DEFAULT_DbFile = $"{nameof(CdnGetter)}.db";

    /// <summary>
    /// Specifies path of database file.
    /// </summary>
    /// <remarks>This pathis relative to the <see cref="Microsoft.Extensions.Hosting.ContentRootPath" />. The default value of this setting is defined in the <see cref="DEFAULT_DbFile" /> constant.</remarks>
    public string? DbFile  { get; set; }
    
    /// <summary>
    /// Configuration settings for the <see cref="CdnJsRemoteService" />.
    /// </summary>
    public CdnJsSettings? CdnJs { get; set; }

    /// <summary>
    /// The <see cref="Model.RemoteService.Name" /> of the upstream content delivery service.
    /// </summary>
    public List<string>? Upstream { get; set; }

    /// <summary>
    /// Gets the library versions.
    /// </summary>
    public List<string>? Version { get; set; }

    /// <summary>
    /// Gets the names of libraries on the upstream CDN to be added to the database.
    /// </summary>
    /// <remarks>
    ///     Mandatory Switches
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:AddLibrary=<c>name[,name,...]</c></term>
    ///             <description>The library name(s) on the remote CDN to be added to the database.</description>
    ///         </item>
    ///         <item>
    ///             <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///             <description>The upstream CDN name(s) to retrieve libraries from.</description>
    ///         </item>
    ///     </list>
    ///     Optional Switch
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///             <description>The specific version(s) to add. If this is not specified, then all versions will be added.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public List<string>? AddLibrary { get; set; }

    /// <summary>
    /// Gets names of libraries in the database that are to be checked for new versions on the remote CDN.
    /// </summary>
    /// <remarks>
    ///     Mandatory Switches
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:GetNewVersions=<c>library_name[,library_name,...]</c></term>
    ///             <description>The library name(s) on the remote CDN to be added to the database.</description>
    ///         </item>
    ///     </list>
    ///     Optional Switch
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///             <description>The upstream CDN name(s) to retrieve libraries from. If this is not specified, then new versions will be retrieved from all CDNs.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public List<string>? GetNewVersions { get; set; }

    /// <summary>
    /// Gets names of libraries to remove from the database.
    /// </summary>
    /// <remarks>
    ///     Parameter Set #1: Mandatory Switch
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:RemoveLibrary=<c>name[,name,...]</c></term>
    ///             <description>The library name(s) to remove from the database.</description>
    ///         </item>
    ///     </list>
    ///     Optional Switches
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///             <description>The explicit upstream CDN name(s) to remove local libraries from. If this is not specified, then all matching libraries will be removed.</description>
    ///         </item>
    ///         <item>
    ///             <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///             <description>The specific version(s) to remove. If this is not specified, then all versions of matching libraries will be removed.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public List<string>? RemoveLibrary { get; set; }

    /// <summary>
    /// Gets names of libraries in the database that are to be reloaded from the remote CDN.
    /// </summary>
    /// <remarks>
    ///     Parameter Set #1: Mandatory Switches
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--RCdnGetter:eloadLibrary=<c>name[,name,...]</c></term>
    ///             <description>The library name(s) on the remote CDN to be reloaded.</description>
    ///         </item>
    ///         <item>
    ///             <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///             <description>The upstream CDN name(s) to retrieve libraries from.</description>
    ///         </item>
    ///     </list>
    ///     Optional Switch
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///             <description>The specific version(s) to reload. If this is not specified, then all versions of matching libraries will be reloaded.</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Parameter Set #2: Mandatory Switches
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:ReloadLibrary=<c>name[,name,...]</c></term>
    ///                 <description>The library name(s) to be reloaded.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///                 <description>The specific version(s) to reload.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public List<string>? ReloadLibrary { get; set; }

    /// <summary>
    /// Gets names of libraries in the database whose existing library versions are to be reloaded from the remote CDN.
    /// </summary>
    /// <remarks>
    ///     Parameter Set #1: Mandatory Switches
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:ReloadLibrary=<c>name[,name,...]</c></term>
    ///             <description>The library name(s) on the remote CDN to be reloaded.</description>
    ///         </item>
    ///         <item>
    ///             <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///             <description>The upstream CDN name(s) to retrieve libraries from.</description>
    ///         </item>
    ///     </list>
    ///     Optional Switch
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///             <description>The specific version(s) to reload. If this is not specified, then all versions of matching libraries will be reloaded.</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Parameter Set #2: Mandatory Switches
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:ReloadLibrary=<c>name[,name,...]</c></term>
    ///                 <description>The library name(s) to be reloaded.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///                 <description>The specific version(s) to reload.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public List<string>? ReloadExistingVersions { get; set; }

    /// <summary>
    /// The <see cref="Model.LocalLibrary.Name" />(s) of the local librares.
    /// </summary>
    public List<string>? Library { get; set; }

    public const string SHOW_Remotes = "Remotes";
    
    public const string SHOW_Libraries = "Libraries";
    
    public const string SHOW_Versions = "Versions";
    
    public const string SHOW_Files = "Files";
    
    /// <summary>
    /// Display information.
    /// </summary>
    /// <remarks>
    ///     Show Remotes
    ///     <list type="bullet">
    ///         <item>
    ///             <term>--CdnGetter:Show=Remotes</term>
    ///             <description>Show the remote CDN names in the database.</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         Show Libraries
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:Show=Libraries</term>
    ///                 <description>Show the remote CDN names in the database.</description>
    ///             </item>
    ///         </list>
    ///         Optional Parameter
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///                 <description>The upstream CDN name(s) to show local libraries for.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Show Versions
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:Show=Versions</term>
    ///                 <description>Show the remote CDN names in the database.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Library=<c>name[,name,...]</c></term>
    ///                 <description>The library name(s) to show versions for.</description>
    ///             </item>
    ///         </list>
    ///         Optional Parameter
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///                 <description>The upstream CDN name(s) to show local libraries for.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Show Files
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>--CdnGetter:Show=Files</term>
    ///                 <description>Show the remote CDN file names in the database.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Library=<c>name[,name,...]</c></term>
    ///                 <description>The library name(s) to show versions for.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Version=<c>string[,string,...]</c></term>
    ///                 <description>The library name(s) to show versions for.</description>
    ///             </item>
    ///             <item>
    ///                 <term>--CdnGetter:Upstream=<c>name[,name,...]</c></term>
    ///                 <description>The library name(s) to show versions for.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public string? Show { get; set; }

    public static string GetDbFileName(AppSettings? settings) { return (settings?.DbFile).ToTrimmedOrDefaultIfEmpty(DEFAULT_DbFile); }
}
