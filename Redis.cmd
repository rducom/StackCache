.nuget\nuget.exe install redis-64 -excludeversion
START /B redis-64\redis-server.exe --maxheap 512M  > redislog.txt