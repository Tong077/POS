using Microsoft.Data.SqlClient;
using System.Data;

namespace POS_System.Data
{
    public class DapperConnection
    {
        private readonly IConfiguration _configuration;

        public DapperConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("MyConnection"));
    }
}
