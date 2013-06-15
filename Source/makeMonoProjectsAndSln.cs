using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// Convert a C# windows soltion so that it can easily be built on mono. I've found this easier than trying to get a single solution working on both
// This script will
//   1) Create a .mono.sln from the sln
//      1.1) Change the file version from 12 to 11 for MonoDevelop
//      1.2) Change all project includes from .csproj to .mono.csproj
//   2) Create a .mono.csproj from each .csproj
//     2.1) Add ProductVersion and SchemaVersion
//     2.2) Change the output directory from Debug to monoDebug and Release to monoRelease
//     2.3) Add mono to the start of any prebuild event so that mono executables can be run - probably too sepcific for everyone but it is useful
//     2.3) Change nuget.targets to nuget.mono.targets
public class SoltionAndProjectConverter
{
	//Pass solution path as only argument
	public static void Main( string[] args )
	{
		var slnTxt = File.ReadAllText( args[0] );
		
		//Change the file version so that MonoDevelop will open the solution
		slnTxt = slnTxt.Replace( @"Microsoft Visual Studio Solution File, Format Version 12.00", @"Microsoft Visual Studio Solution File, Format Version 11.00" );
		
		bool matched = false;
		
		//Change all .csproj to .mono.csproj checking that they are not already .mono.csproj (they should not be)
		do
		{
			matched = false;
			
			var matches = Regex.Matches( slnTxt, @"\\(?<prj>.*?).csproj""" );
			
			foreach( Match match in matches )
			{
				if( match.Groups["prj"].Value.Contains( ".mono" ) )
				{
					continue;
				}
				
				matched = true;
				
				slnTxt = slnTxt.Substring( 0, match.Index ) + 
					"\\" +
					match.Groups["prj"].Value +
					".mono" + 
					slnTxt.Substring( match.Index + match.Groups["prj"].Value.Length + 1 );
				
				break;
			}

		}while( matched );
		
		string newPath = Path.Combine( Path.GetDirectoryName( args[0] ), Path.GetFileNameWithoutExtension( args[0] ) + ".mono.sln" );
		File.WriteAllText( newPath, slnTxt );
		
		
		//Process all projects
		foreach( var prj in Directory.GetFiles( ".", "*.csproj", SearchOption.AllDirectories ) )
		{
			if( prj.ToLower().EndsWith( ".mono.csproj" ) )
			{
				continue;
			}
			
			Console.WriteLine( prj );
			
			var prjTxt = File.ReadAllText( prj );
			
			prjTxt = Regex.Replace( 
				prjTxt, 
				@"\<TargetFrameworkVersion\>[^\<]+?\</TargetFrameworkVersion\>", 
				@"<ProductVersion>10.0.0</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>",
				RegexOptions.Multiline	);
			
			prjTxt = Regex.Replace( 
				prjTxt, 
				@"Release(\\?)<\/OutputPath\>", 
				@"monoRelease\</OutputPath>",
				RegexOptions.Multiline	);

			prjTxt = Regex.Replace( 
				prjTxt, 
				@"Debug(\\?)<\/OutputPath\>", 
				@"monoDebug\</OutputPath>",
				RegexOptions.Multiline	);

			prjTxt = prjTxt.Replace( 
				@"<PreBuildEvent>$(SolutionDir)", 
				@"<PreBuildEvent>mono $(SolutionDir)"	);
			
			prjTxt = prjTxt.Replace( 
				@"<PreBuildEvent>""$(SolutionDir)", 
				@"<PreBuildEvent>mono ""$(SolutionDir)"	);
			
			prjTxt = prjTxt.Replace( 
				@"<SolutionDir Condition=""$(SolutionDir) == '' Or $(SolutionDir) == '*Undefined*'"">..\</SolutionDir>", 
				@""	);
			
			prjTxt = prjTxt.Replace( ".csproj\">", ".mono.csproj\">" );
			prjTxt = prjTxt.Replace( @".nuget\nuget.targets", @".nuget\nuget.mono.targets" );
			
			string newPathPrj = Path.Combine( Path.GetDirectoryName( prj ), Path.GetFileNameWithoutExtension( prj ) + ".mono.csproj" );
			Console.WriteLine( "   " + newPathPrj );
			File.WriteAllText( newPathPrj, prjTxt );
		}
	}
}