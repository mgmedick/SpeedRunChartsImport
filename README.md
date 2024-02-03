How this is being used in Production currently:

Crontab:
*/15 * * * * /usr/bin/flock -n /tmp/speedrunappimport.lockfile bash /srv/speedrunappimport_job.sh
0 0 1 * * /usr/bin/flock -n /tmp/speedrunappimport.lockfile bash /srv/speedrunappimportfull_job.sh

speedrunappimport_job.sh:
#!/bin/bash
cd /srv/speedrunappimport/ && dotnet SpeedRunAppImport.dll ProcessIDs=0 >> /var/log/speedrunappimport/speedrunappimport-"`date +"%d-%m-%Y"`".log 2>&1

speedrunappimportfull_job.sh:
#!/bin/bash
cd /srv/speedrunappimport/ && dotnet SpeedRunAppImport.dll IsBulkReload=true IsUpdateSpeedRuns=true ProcessIDs=0 >> /var/log/speedrunappimport/speedrunappimportfull-"`date +"%d-%m-%Y"`".log 2>&1
