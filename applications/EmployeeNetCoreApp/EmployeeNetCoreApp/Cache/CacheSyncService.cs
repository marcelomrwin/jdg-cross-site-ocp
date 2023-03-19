using System;
using EmployeeNetCoreApp.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeNetCoreApp.Cache
{
    public class CacheSyncService : BackgroundService
	{
        private readonly ILogger<CacheSyncService> logger;        
        public IServiceProvider services;

        public CacheSyncService(IServiceProvider services, ILogger<CacheSyncService> pLogger)
		{
            this.services = services;            
            logger = pLogger;
		}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
            while(await timer.WaitForNextTickAsync(stoppingToken))
            {
                using(var scope = services.CreateScope())
                {
                    try {
                        var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
                        logger.LogInformation("Event START!!");
                        await employeeService.SyncCacheWithDatabase(stoppingToken);
                    }catch(Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
                    finally
                    {
                        logger.LogInformation("Event END!!");
                    }
                                        
                }                
            }
        }
    }
}

