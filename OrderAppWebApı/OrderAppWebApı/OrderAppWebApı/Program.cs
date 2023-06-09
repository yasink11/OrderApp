using Microsoft.EntityFrameworkCore;
using OrderAppWebAp�.BackgroundService;
using OrderAppWebAp�.Context;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region MyServices
string connetionString = builder.Configuration.GetConnectionString("MySql");
builder.Services.AddDbContext<OrderContextDb>(opt =>
{
    opt.UseMySql(connetionString, ServerVersion.AutoDetect(connetionString), null);
});

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

builder.Services.AddHostedService<SendMailService>();


Logger log = new LoggerConfiguration()
    .WriteTo.File("logs/log.txt")
    .WriteTo.MySQL(connetionString, "logs")
    .MinimumLevel.Information()
    .CreateLogger();

builder.Host.UseSerilog(log);

#endregion


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
