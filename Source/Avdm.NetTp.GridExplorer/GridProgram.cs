using System;
using Avdm.Config;
using Avdm.Core.NancyFx;
using Avdm.NetTp.Core;
using Nancy.Hosting.Self;

namespace Avdm.NetTp.GridExplorer
{
    public class GridProgram
    {
        static void Main( string[] args )
        {
            StandardInitialiser.Configure();

            var port = int.Parse( ConfigManager.AppSettings["GridExplorer.Port"] ?? "8092" );
            var host = new NancyHost( new GridExplorerBootstrapper(), NancyHelper.GetUriParams( port ) );
            host.Start();

            new GridModule();

            Console.WriteLine( "Grid explorer running on port {0}", port );
            Console.ReadLine();
        }
    }
}
