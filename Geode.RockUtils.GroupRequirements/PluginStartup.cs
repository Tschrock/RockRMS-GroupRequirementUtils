using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Hosting;

using DotLiquid;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

using Geode.RockUtils.GroupRequirements.Web;

namespace Geode.RockUtils.GroupRequirements
{
    public class PluginStartup : IRockStartup
    {
        /// <summary>
        /// The startup order.
        /// For this plugin, we don't care what order we're started in.
        /// </summary>
        public int StartupOrder => 0;

        /// <summary>
        /// Runs when Rock starts up.
        /// </summary>
        public void OnStartup()
        {
            // Register safe types for lava.
            // We use these in our templates but Rock doesn't include them by default.
            RegisterSafeType( typeof( PersonGroupRequirementStatus ) );
            RegisterSafeType( typeof( GroupRequirementStatus ) );

            // Register our virtual path provider.
            // This allows Rock to load blocks that we've bundled as resources.
            HostingEnvironment.RegisterVirtualPathProvider( new AssemblyResourceProvider() );

            // Make sure our blocks are registered.
            // This tells Rock about any blocks we've bundled.
            RegisterBundledBlockTypes( true );
        }

        /// <summary>
        /// Registers a type with the Lava templating system.
        /// </summary>
        /// <param name="type"></param>
        private void RegisterSafeType( Type type )
        {
            Template.RegisterSafeType( type, type.GetProperties().Select( p => p.Name ).ToArray() );
        }

        /// <summary>
        /// Registers bundled block types.
        /// </summary>
        private void RegisterBundledBlockTypes( bool refreshAll = false )
        {
            // Pre-fetch for later
            Type rockBlockType = typeof( Rock.Web.UI.RockBlock );
            int? blockEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Block ) ).Id;

            // Get all blocks bundled as resources
            var blockResourceNames = Meta.AssemblyResourceNames.Where( n => n.EndsWith( ".ascx" ) );

            // Get all registered block paths
            var registeredPaths = new List<string>();
            using ( var rockContext = new RockContext() )
            {
                registeredPaths = new BlockTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( b => !string.IsNullOrEmpty( b.Path ) )
                    .Select( b => b.Path )
                    .ToList();
            }

            // For each bundled block
            foreach ( string resourceName in blockResourceNames )
            {
                // Get it's virtual path
                string path = Meta.ResourceNameToVirtualPath( resourceName );

                // If it hasn't been registered yet
                if ( refreshAll || !registeredPaths.Any( b => b.Equals( path, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // Attempt to load the control
                    try
                    {
                        // Compile the control
                        var blockCompiledType = System.Web.Compilation.BuildManager.GetCompiledType( path );

                        // Check that it compiled and inherits Rock.Web.UI.RockBlock
                        if ( blockCompiledType != null && rockBlockType.IsAssignableFrom( blockCompiledType ) )
                        {
                            // Add or update it's BlockType
                            using ( var rockContext = new RockContext() )
                            {
                                var blockTypeService = new BlockTypeService( rockContext );

                                // Look for an existing BlockType
                                var blockType = blockTypeService.Queryable().FirstOrDefault( b => b.Path == path );
                                if ( blockType == null )
                                {
                                    // Create new BlockType record and save it
                                    blockType = new BlockType
                                    {
                                        Path = path
                                    };
                                    blockTypeService.Add( blockType );
                                }

                                // Update the block's Name
                                blockType.Name = Reflection.GetDisplayName( blockCompiledType ) ?? resourceName;

                                if ( blockType.Name.Length > 100 )
                                {
                                    blockType.Name = blockType.Name.Truncate( 100 );
                                }

                                // Update the block's Category
                                blockType.Category = Reflection.GetCategory( blockCompiledType ) ?? string.Empty;

                                // Update the block's Description
                                blockType.Description = Reflection.GetDescription( blockCompiledType ) ?? string.Empty;

                                // Save the block
                                rockContext.SaveChanges();

                                // Update the attributes used by the block
                                Rock.Attribute.Helper.UpdateAttributes( blockCompiledType, blockEntityTypeId, "BlockTypeId", blockType.Id.ToString(), rockContext );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"RegisterBlockTypes failed for {path} with exception: {ex.Message}" );
                        ExceptionLogService.LogException( new Exception( string.Format( "Problem processing block with path '{0}'.", path ), ex ), null );
                    }
                }
            }
        }
    }
}
