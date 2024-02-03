How this is being used in Production currently:

Runs every 15 mins:
dotnet SpeedRunAppImport.dll ProcessIDs=0

Runs on first of every month:
dotnet SpeedRunAppImport.dll IsBulkReload=true IsUpdateSpeedRuns=true ProcessIDs=0 

