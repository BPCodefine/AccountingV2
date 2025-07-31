using AccountingV2.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DBAccess>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles(); // Serve index.html from wwwroot
app.UseStaticFiles();  // Serve Angular files
app.MapGeneralEndpoints();
app.MapAccountingEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
