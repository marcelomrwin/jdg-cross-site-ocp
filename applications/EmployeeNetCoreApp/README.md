### SQL Server
```
docker run --rm -d --name sqlserver -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Passw0rd1#" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
npm install -g sql-cli
mssql -u sa -p Passw0rd1#
  mssql> select @@version
Use THE GUI APPLICATION - AZURE DATA STUDIO

```
#### Create new database https://adamtheautomator.com/azure-data-studio/#Creating_a_New_Database
-> New query  
```sql
USE master
GO

IF NOT EXISTS (
    SELECT name
    FROM sys.databases
    WHERE name = N'DEMODB'
)
    CREATE DATABASE [DEMODB];
GO
```

### DotNet App https://stackup.hashnode.dev/ef-migrations-visual-studio-mac
```
dotnet new tool-manifest
dotnet tool install --local dotnet-ef
dotnet ef migrations add DbInitializationWithSeed
dotnet ef database update
dotnet ef migrations remove
```﻿
