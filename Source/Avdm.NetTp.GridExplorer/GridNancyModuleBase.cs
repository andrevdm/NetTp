using System.IO;
using System.Reflection;
using Nancy;

namespace Avdm.NetTp.GridExplorer
{
    public class GridNancyModuleBase : NancyModule
    {
        public GridNancyModuleBase()
            : base("/grid")
        {
        }

        protected Stream GetFile( string fileName )
        {
            var filePath = Path.Combine( @"C:\Development\NetTp\Source\NetTp.GridExplorer\Resources\", fileName.Replace( "/", "\\" ) );

            if( File.Exists( filePath ) )
            {
                return File.OpenRead( filePath );
            }
            else
            {
                var nsAndName = GetType().Namespace + ".Resources." + fileName.ToLower();
                return Assembly.GetExecutingAssembly().GetManifestResourceStream( nsAndName );
            }
        }
    }
}