REM set myKey=
set ver=0.0.1

nuget push bin\Packages\Release\NuGet\PivotExpert.%ver%.nupkg -src https://api.nuget.org/v3/index.json -ApiKey %myKey%
