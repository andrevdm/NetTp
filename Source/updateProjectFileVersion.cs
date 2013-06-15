using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

//From http://trycatch.me/automatically-update-the-assemblyfileversion-attribute-of-a-net-assembly/ but starting powershell for each build was too slow.
//Pass the C# project path as the only parameter
public class Updater
{
    public static void Main( string[] args )
    {
		if( args.Length > 0 )
		{
			string path = args[0];
			Update( path );
		}
		else
		{
			foreach( var prj in Directory.GetFiles( ".", "AssemblyInfo.cs", SearchOption.AllDirectories ) )
			{
				Update( prj );
			}
		}
	 }
	 
	 private static void Update( string path )
	 {
		var txt = File.ReadAllText( path );
		var asmMatch = Regex.Match( txt, @"^\s*\[\s*assembly\s*:\s*AssemblyVersion\s*\(\s*""(?<major>\d+)\.(?<minor>\d+)\.[^""]+?""\s*\)\s*\]", RegexOptions.Multiline );
 		var major = asmMatch.Groups["major"].Value ?? "1";  
		var minor = asmMatch.Groups["minor"].Value ?? "0";
		int build = ((DateTime.Now.Year - 2000) * 366) + DateTime.Now.DayOfYear;
		int revision = (int)(DateTime.Now - DateTime.Now.Date).TotalSeconds / 2;
		
		string ver = string.Format( 
				@"[assembly: AssemblyFileVersion(""{0}.{1}.{2}.{3}"")]",
				major,
				minor,
				build,
				revision );

		 Console.WriteLine( "Updating file version: {0} to {1}", path, ver );
		 
		 txt = Regex.Replace( 
			txt, 
			@"^\s*\[\s*assembly\s*:\s*AssemblyFileVersion\s*\(\s*""[^"")]*?""\s*\)\s*\]", 
			ver,
			RegexOptions.Multiline	);
		 
		 File.WriteAllText( path, txt );
    }
}