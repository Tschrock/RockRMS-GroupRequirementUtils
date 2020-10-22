using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Geode.RockUtils.GroupRequirements
{
    /// <summary>
    /// Static helpers for managing assembly resources.
    /// </summary>
    internal static class Meta
    {
        /// <summary>
        /// The plugin assembly.
        /// </summary>
        internal static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// The name of the assembly.
        /// </summary>
        internal static readonly string AssemblyName = Assembly.GetName().Name;

        /// <summary>
        /// The folder prefix for resources in this assembly.
        /// </summary>
        internal static readonly string AssemblyResourcePrefix = $"~/{AssemblyName}.dll/";

        /// <summary>
        /// A list of resources in this assembly.
        /// </summary>
        internal static readonly string[] AssemblyResourceNames = Assembly.GetManifestResourceNames();

        /// <summary>
        /// Checks if the given resource exists in this assembly.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>A boolean value indicating if the given resource exists in this assembly.</returns>
        internal static bool ResourceExists( string name )
        {
            return AssemblyResourceNames.Contains( name );
        }

        /// <summary>
        /// Opens a stream to the given resource.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>A Stream to the given resource, or null if it does not exist.</returns>
        internal static Stream OpenResource( string name )
        {
            return Assembly.GetManifestResourceStream( name );
        }

        /// <summary>
        /// Converts a virtual path to a resource name.
        /// </summary>
        /// <param name="path">The virtual path.</param>
        /// <returns>The resource name.</returns>
        internal static string VirtualPathToResourceName( string path )
        {
            string relativePath = VirtualPathUtility.ToAppRelative( path );
            string[] pathParts = relativePath.TrimStart('~').TrimStart('/').Split( '/' );
            return pathParts.Length == 2 ? pathParts[1] : "invalid";
        }

        /// <summary>
        /// Converts a resource name to a virtual path.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <returns>The virtual path.</returns>
        internal static string ResourceNameToVirtualPath( string name )
        {
            return AssemblyResourcePrefix + name;
        }
    }
}
