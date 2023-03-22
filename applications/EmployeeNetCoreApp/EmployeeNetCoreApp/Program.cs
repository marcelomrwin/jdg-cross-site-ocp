using EmployeeNetCoreApp.Cache;
using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Services;
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
DataGridRestClient dataGridRestClient = new DataGridRestClient(cacheConfiguration);
builder.Services.AddSingleton<DataGridRestClient>(dataGridRestClient);

builder.Services.AddLogging(option =>
{
    option.AddConsole(c =>
    {
        c.TimestampFormat = "[yyyy/MM/dd HH:mm:ss]";                
    });
});


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

