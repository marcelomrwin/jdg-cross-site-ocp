using EmployeeNetCoreApp.Cache;
using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Services;
using Infinispan.Hotrod.Caching.Distributed;
using Infinispan.Hotrod.Core;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add env vars https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var cacheConfiguration = builder.Configuration.GetSection("CacheConfiguration").Get<CacheConfiguration>();
builder.Services.AddSingleton(cacheConfiguration);

//Infinispan

InfinispanDG infinispan = new InfinispanDG();
infinispan.AddHost(cacheConfiguration.Host, cacheConfiguration.Port);
infinispan.Password = cacheConfiguration.Password;
infinispan.User = cacheConfiguration.User;
infinispan.AuthMech = "PLAIN";
infinispan.ClientIntelligence = 0x03;

builder.Services.AddSingleton(infinispan);

builder.Services.AddInfinispanCache(options =>
{
    options.CacheName = cacheConfiguration.Cache;    
    options.Cluster = infinispan;    

    options.Cluster.AddHost(cacheConfiguration.Host, cacheConfiguration.Port);
    options.Cluster.Password = cacheConfiguration.Password;
    options.Cluster.User = cacheConfiguration.User;
});

builder.Services.AddSingleton<CacheSyncService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<CacheSyncService>());


var app = builder.Build();

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

app.MapControllers();

app.Run();

