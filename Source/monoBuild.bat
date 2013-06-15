@echo off

makeMonoProjectsAndSln.exe NetTp.Framework.sln
"C:\Program Files (x86)\Mono-3.0.10\bin\xbuild.bat" /p:TargetFrameworkProfile="" NetTp.Framework.mono.sln
