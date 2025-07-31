using Dapper;

namespace AccountingV2.Endpoints
{
    public static class GeneralEndpoints
    {
        public static void MapGeneralEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/username", (HttpContext context) =>
            {
                var username = context.User.Identity?.Name;
                return Results.Ok(new
                {
                    username
                });
            }).RequireAuthorization();

            app.MapGet("/api/dbtest", (DBAccess context) =>
            {
                using var conn = context.Create();

                string? username = conn.ExecuteScalar<string>("SELECT SYSTEM_USER");

                return Results.Ok(new { sqlUser = username });
            });
        }
    }
}
