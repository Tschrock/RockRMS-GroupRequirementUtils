using System.IO;
using System.Web.Hosting;

namespace Geode.RockUtils.GroupRequirements.Web
{
    /// <summary>
    /// A virtual file bundled into an assembly's resources.
    /// </summary>
    class AssemblyResourceVirtualFile : VirtualFile
    {
        /// <summary>
        /// Creates a new AssemblyResourceVirtualFile.
        /// </summary>
        /// <param name="virtualPath"></param>
        public AssemblyResourceVirtualFile( string virtualPath ) : base( virtualPath )
        {
        }

        /// <inheritdoc/>
        public override Stream Open()
        {
            return Meta.OpenResource( Meta.VirtualPathToResourceName( VirtualPath ) );
        }
    }
}
