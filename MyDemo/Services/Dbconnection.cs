using System.Data.SqlClient;

namespace MyDemo.Services;
public interface IDbconnection
{
    Task<SqlConnection> GetConnectionAsync();
}

public class Dbconnection:IDbconnection
{
    private readonly IConfiguration _config;
    public Dbconnection( IConfiguration config ){
        _config=config;
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        return connection;
    }

}

    
