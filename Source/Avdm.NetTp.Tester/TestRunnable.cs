using System;
using Avdm.Core.Console;
using Avdm.NetTp.Core;

namespace Avdm.NetTp.Tester
{
    public class TestRunnable : IRunnable
    {
        public int Run( string[] args )
        {
            Console.WriteLine( "Runnable args={0}", string.Join( ", ", args ) );
            Console.WriteLine( "Press return to exit" );
            Console.ReadLine();
            return 0;
        }
    }
}