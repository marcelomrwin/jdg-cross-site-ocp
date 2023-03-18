# BUILD STAGE
FROM registry.redhat.io/ubi8/dotnet-70
WORKDIR /opt/app-root/src
USER 0
RUN mkdir -p EmployeeNetCoreApp/bin
RUN chown -R 1001:0 /opt/app-root && fix-permissions /opt/app-root
USER 1001
COPY ["EmployeeNetCoreApp/EmployeeNetCoreApp.csproj", "EmployeeNetCoreApp/"]
RUN dotnet restore "EmployeeNetCoreApp/EmployeeNetCoreApp.csproj"
COPY . .
WORKDIR "/opt/app-root/src/EmployeeNetCoreApp"
RUN dotnet build "EmployeeNetCoreApp.csproj" -c Release -o /opt/app-root/build
RUN dotnet publish "EmployeeNetCoreApp.csproj" -c Release -o /opt/app-root/publish /p:UseAppHost=false
# RUNTIME STAGE
FROM registry.redhat.io/ubi8/dotnet-70-runtime
COPY --from=0 /opt/app-root/publish .
ENTRYPOINT ["dotnet", "EmployeeNetCoreApp.dll"]
