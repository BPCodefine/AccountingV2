using AccountingV2.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DBAccess>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles(); // Serve index.html from wwwroot
app.UseStaticFiles();  // Serve Angular files
app.MapGeneralEndpoints();
app.MapAccountingEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
