set version=1.0.8

dotnet build "../../SqlSugar.csproj" -p:PackageVersion=%version%

%~dp0nuget.exe pack %~dp0SqlSugarForCore.nuspec -OutputDirectory %~dp0

dotnet nuget push "%~dp0WellSqlSugar5_1_4_94.%version%.nupkg" -k well.123 -s http://123.57.57.116/WellNuGetService/nuget

del /f /s /q "%~dp0WellSqlSugar5_1_4_94.%version%.nupkg"

pause