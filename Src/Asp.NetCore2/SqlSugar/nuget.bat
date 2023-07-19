%~dp0nuget.exe pack %~dp0SqlSugarForCore.nuspec -OutputDirectory %~dp0

dotnet nuget delete WellSqlSugar 5.1.4.67 --non-interactive -k well.123 -s http://123.57.57.116/WellNuGetService/nuget

dotnet nuget push "%~dp0WellSqlSugar5_1_4_67.1.1.0.nupkg" -k well.123 -s http://123.57.57.116/WellNuGetService/nuget

del /f /s /q "%~dp0WellSqlSugar5_1_4_67.1.1.0.nupkg"

pause