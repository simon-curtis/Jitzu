using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Runtime.Loader;

namespace Jitzu.Core.Runtime.Compilation;

public class PackageResolver
{
    private static readonly string GlobalCache = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nuget", "packages");

    private readonly SourceCacheContext _cache = new();

    private readonly SourceRepository _nugetOrg = Repository.Factory.GetCoreV3(
        "https://api.nuget.org/v3/index.json");

    // Track loaded package IDs to avoid re-resolving
    private readonly HashSet<string> _resolvedPackages = new(StringComparer.OrdinalIgnoreCase);

    private bool _resolverRegistered;

    public PackageResolver()
    {
        RegisterAssemblyResolver();
    }

    /// <summary>
    /// Registers a fallback assembly resolver that looks in the NuGet global cache.
    /// </summary>
    private void RegisterAssemblyResolver()
    {
        if (_resolverRegistered) return;
        _resolverRegistered = true;

        AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
        {
            // Search the global cache for matching assembly
            var assemblyFileName = assemblyName.Name + ".dll";

            // Look through all packages in the cache
            if (!Directory.Exists(GlobalCache))
                return null;

            foreach (var packageDir in Directory.GetDirectories(GlobalCache))
            {
                foreach (var versionDir in Directory.GetDirectories(packageDir))
                {
                    // Search recursively for the assembly
                    var found = Directory
                        .EnumerateFiles(versionDir, assemblyFileName, SearchOption.AllDirectories)
                        .FirstOrDefault();

                    if (found != null)
                    {
                        try
                        {
                            return context.LoadFromAssemblyPath(found);
                        }
                        catch
                        {
                            // Continue searching
                        }
                    }
                }
            }

            return null;
        };
    }

    public async Task<string[]> ResolveAsync(
        string packageId,
        NuGetVersion version,
        NuGetFramework target,
        CancellationToken ct = default)
    {
        var allAssemblies = new List<string>();
        var toResolve = new Queue<PackageIdentity>();
        toResolve.Enqueue(new PackageIdentity(packageId, version));

        var findResource = await _nugetOrg.GetResourceAsync<FindPackageByIdResource>(ct);
        var dependencyResource = await _nugetOrg.GetResourceAsync<DependencyInfoResource>(ct);

        while (toResolve.Count > 0)
        {
            var package = toResolve.Dequeue();
            var cacheKey = $"{package.Id}/{package.Version}".ToLowerInvariant();

            if (!_resolvedPackages.Add(cacheKey))
                continue; // Already resolved

            var packagePath = Path.Combine(GlobalCache, package.Id.ToLower(), package.Version.ToString());
            var assemblies = await ExtractPackageAsync(
                findResource, package.Id, package.Version, packagePath, target, ct);

            allAssemblies.AddRange(assemblies);

            // Resolve dependencies
            var depInfo = await dependencyResource.ResolvePackage(
                package, target, _cache, NullLogger.Instance, ct);

            if (depInfo?.Dependencies != null)
            {
                foreach (var dep in depInfo.Dependencies)
                {
                    // Use the minimum version from the range
                    var depVersion = dep.VersionRange.MinVersion ?? new NuGetVersion("0.0.0");
                    
                    // Try to find best matching version if we need higher
                    if (dep.VersionRange.MinVersion == null || !dep.VersionRange.Satisfies(depVersion))
                    {
                        var versions = await findResource.GetAllVersionsAsync(
                            dep.Id, _cache, NullLogger.Instance, ct);
                        
                        depVersion = versions
                            .Where(v => dep.VersionRange.Satisfies(v))
                            .OrderByDescending(v => v)
                            .FirstOrDefault() ?? depVersion;
                    }

                    toResolve.Enqueue(new PackageIdentity(dep.Id, depVersion));
                }
            }
        }

        return allAssemblies.ToArray();
    }

    private async Task<List<string>> ExtractPackageAsync(
        FindPackageByIdResource resource,
        string packageId,
        NuGetVersion version,
        string packagePath,
        NuGetFramework target,
        CancellationToken ct)
    {
        var assemblies = new List<string>();

        // Check if already extracted
        if (Directory.Exists(packagePath))
        {
            var existing = Directory.GetFiles(packagePath, "*.dll", SearchOption.AllDirectories);
            if (existing.Length > 0)
            {
                // Filter to the correct framework folder
                var libPath = Path.Combine(packagePath, "lib");
                if (Directory.Exists(libPath))
                {
                    assemblies.AddRange(FindBestFrameworkAssemblies(libPath, target));
                }
                
                if (assemblies.Count == 0)
                    assemblies.AddRange(existing);
                
                return assemblies;
            }
        }

        using var stream = new MemoryStream();
        var success = await resource.CopyNupkgToStreamAsync(
            packageId, version, stream, _cache, NullLogger.Instance, ct);

        if (!success)
            return assemblies;

        stream.Position = 0;
        using var reader = new PackageArchiveReader(stream);

        var libItems = (await reader.GetLibItemsAsync(ct)).ToList();
        var nearest = NuGetFrameworkUtility.GetNearest(libItems, target);

        if (nearest == null)
            return assemblies; // No compatible framework, skip

        Directory.CreateDirectory(packagePath);

        foreach (var file in nearest.Items.Where(f => f.EndsWith(".dll")))
        {
            var destPath = Path.Combine(packagePath, file);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

            await using var entryStream = await reader.GetStreamAsync(file, ct);
            await using var fileStream = File.Create(destPath);
            await entryStream.CopyToAsync(fileStream, ct);

            assemblies.Add(destPath);
        }

        return assemblies;
    }

    private static List<string> FindBestFrameworkAssemblies(string libPath, NuGetFramework target)
    {
        var frameworks = Directory.GetDirectories(libPath)
            .Select(d => new { Path = d, Framework = NuGetFramework.ParseFolder(Path.GetFileName(d)) })
            .Where(x => DefaultCompatibilityProvider.Instance.IsCompatible(target, x.Framework))
            .OrderByDescending(x => x.Framework, NuGetFrameworkSorter.Instance)
            .ToList();

        if (frameworks.Count == 0)
            return [];

        return Directory.GetFiles(frameworks[0].Path, "*.dll").ToList();
    }
}