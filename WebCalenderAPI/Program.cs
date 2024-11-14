using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Json;
using System.Runtime.Serialization;
using System.Text;
using WebCalenderAPI.Data;
using WebCalenderAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MyDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("MyDB"));

});
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Host.UseSerilog((ctx, config) =>
{
    config.WriteTo.Console().MinimumLevel.Information();
    config.WriteTo.File(
          path: AppDomain.CurrentDomain.BaseDirectory + "/logs/log-.txt",
          rollingInterval : RollingInterval.Day,
          rollOnFileSizeLimit: true,
          formatter: new JsonFormatter()).MinimumLevel.Information();

});

var secretKey = builder.Configuration["AppSettings:SecretKey"];
var scretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(scretKeyBytes),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddDistributedRedisCache(
    options =>
    {
        options.Configuration = "localhost:6379";
    }
    );

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
