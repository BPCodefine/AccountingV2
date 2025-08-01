using Dapper;

namespace AccountingV2.Endpoints
{
    public static class GeneralEndpoints
    {
        public static void MapGeneralEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/username", (HttpContext context) =>
            {
                string userName;
                var fullName = context.User?.Identity?.Name;
#if DEBUG
                userName = string.IsNullOrWhiteSpace(fullName)? "bp" : (fullName.Contains('\\') ? fullName.Split('\\').Last() : fullName);
#else
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return Results.Problem("Windows username could not be determined.", statusCode: 500);
                }
                userName = fullName.Contains('\\') ? fullName.Split('\\').Last() : fullName;
#endif
                return Results.Ok(new { userName });
            })
#if !DEBUG
            .RequireAuthorization()
#endif
;

            app.MapGet("/api/dbtest", (DBAccess context) =>
            {
                using var conn = context.Create();

                string? username = conn.ExecuteScalar<string>("SELECT SYSTEM_USER");

                return Results.Ok(new { sqlUser = username });
            });
        }
    }
}
