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
dotnet ef migrations add DbInitializationWithSeed (this value must change after each update)
dotnet ef database update
dotnet ef migrations remove

dotnet add package Infinispan.Hotrod.Caching --prerelease (troquei pelo rest client, estava dando bugs demais)
dotnet add package RestSharp
```

https://developers.redhat.com/articles/2022/07/07/add-infinispan-cache-your-aspnet-application#add_business_code


## Build image
```
docker build --tag marcelodsales/jdg-employee-dotnet -f EmployeeNetCoreApp/Dockerfile .
```

## Openshift
```
oc create -f https://raw.githubusercontent.com/redhat-developer/s2i-dotnetcore/master/dotnet_imagestreams.json
oc new-build --name jdg-employee-dotnet --strategy=docker -D - < EmployeeNetCoreApp/Dockerfile
oc start-build jdg-employee-dotnet --from-dir=. --follow --wait
oc create serviceaccount mssql
oc adm policy add-scc-to-user anyuid -z mssql --as system:admin
oc apply -f mssql-2022-ocp-template.json
oc process --parameters mssql2022
oc new-app --template=mssql2022 -p ACCEPT_EULA=Y -p SA_PASSWORD=Passw0rd1# -p MSSQL_PID=Express -p VOLUME_CAPACITY=1Gi -l app=jdg-employee

oc get pods -l name=mssql -o=custom-columns=NAME:.metadata.name --no-headers

oc port-forward $(oc get pods -l name=mssql -o=custom-columns=NAME:.metadata.name --no-headers) 1433:1433

  mssql -u sa -p Passw0rd1#

    USE master \

    IF NOT EXISTS ( \
        SELECT name \
        FROM sys.databases \
        WHERE name = N'DEMODB' \
    ) \
        CREATE DATABASE [DEMODB];

    RESTORE DATABASE DEMODB from disk=N'DEMODB.bak'

[Optional]{
  oc patch dc/mssql --patch '{"spec":{"template": {"spec":{"serviceAccountName": "mssql"}}}}'
  oc rollout latest mssql  
}

oc new-app jdg-employee-dotnet
oc expose svc/jdg-employee-dotnet
```

## If need to clear the database
```
DROP TABLE dbo.__EFMigrationsHistory;
DROP TABLE dbo.Employees;
DROP TABLE dbo.Departments;
```