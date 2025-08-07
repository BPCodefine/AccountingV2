using AccountingV2.Endpoints;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DBAccess>();
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

#if DEBUG
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});
#else
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
#endif

var app = builder.Build();
#if DEBUG
app.UseCors("AllowAngularClient");
#else
app.UseCors("OpenPolicy");
#endif
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles(); // Serve index.html from wwwroot
app.UseStaticFiles();  // Serve Angular files
app.MapGeneralEndpoints();
app.MapAccountingEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
