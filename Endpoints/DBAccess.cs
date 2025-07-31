using Microsoft.Data.SqlClient;
using System.Data;

namespace AccountingV2.Endpoints
{
    public sealed class DBAccess
    {
        public readonly string connString;
        public readonly string dynDBName;
        public DBAccess(IConfiguration configuration)
        {
            connString = configuration.GetConnectionString("DefaultConnection")!;
            dynDBName = configuration.GetValue<string>("DynamicsDatabaseName") ?? "TRANSPACNAV21.dbo.";
        }
        public IDbConnection Create() => new SqlConnection(connString);
    }
}
