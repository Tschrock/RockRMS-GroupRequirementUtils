using System;
using System.Web;
using System.Web.Hosting;

namespace Geode.RockUtils.GroupRequirements.Web
{
    /// <summary>
    /// A VirtualPathProvider that retrieves files from the current assembly.
    /// </summary>
    public class AssemblyResourceProvider : VirtualPathProvider
    {

        /// <summary>
        /// Creates a new AssemblyResourceProvider.
        /// </summary>
        public AssemblyResourceProvider() { }

        /// <summary>
        /// Returns if the given virtual path is for an assembly resource.
        /// </summary>
        /// <param name="virtualPath">The virual path.</param>
        /// <returns>A boolean value indicating if the given virtual path is for an assembly resource.</returns>
        private bool IsAssemblyResourcePath( string virtualPath )
        {
            string checkPath = VirtualPathUtility.ToAppRelative( virtualPath );
            return checkPath.StartsWith( Meta.AssemblyResourcePrefix, StringComparison.InvariantCultureIgnoreCase );
        }

        /// <inheritdoc/>
        public override bool FileExists( string virtualPath )
        {
            if ( IsAssemblyResourcePath( virtualPath ) )
            {
                return Meta.ResourceExists( Meta.VirtualPathToResourceName( virtualPath ) );
            }
            else
            {
                return Previous.FileExists( virtualPath );
            }
        }

        /// <inheritdoc/>
        public override VirtualFile GetFile( string virtualPath )
        {
            if ( IsAssemblyResourcePath( virtualPath ) )
            {
                return new AssemblyResourceVirtualFile( virtualPath );
            }
            else
            {
                return Previous.GetFile( virtualPath );
            }
        }

        /// <inheritdoc/>
        public override System.Web.Caching.CacheDependency GetCacheDependency(
            string virtualPath,
            System.Collections.IEnumerable virtualPathDependencies,
            DateTime utcStart
        )
        {
            if ( IsAssemblyResourcePath( virtualPath ) )
            {
                return null;
            }
            else
            {
                return Previous.GetCacheDependency( virtualPath, virtualPathDependencies, utcStart );
            }
        }
    }
}
