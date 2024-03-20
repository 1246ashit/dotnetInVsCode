using System.Data.SqlClient;
using MyDemo.Enties;
using Dapper;
namespace MyDemo.Services;

public interface IMySqlService
{
    Task<List<SuperHeroEntity>> GetData();
    Task<int> AddHero(SuperHeroEntity superHero);
    Task<int> DeleteHero(int Id);
    Task<List<UserImage>> GetImagePath(int UID);
}
public class MySqlService : IMySqlService
{
    private readonly IConfiguration _config;
    private readonly IDbconnection _Dbconnection;
    public MySqlService(IConfiguration config, IDbconnection Dbconnection)
    {
        _config = config;
        _Dbconnection = Dbconnection;
    }

    public async Task<List<SuperHeroEntity>> GetData()
    {
        await using var connection = await _Dbconnection.GetConnectionAsync();
        var heroes = await connection.QueryAsync<SuperHeroEntity>("SELECT * FROM SuperHeros");
        return heroes.ToList();
    }

    public async Task<int> AddHero(SuperHeroEntity superHero)
    {
        await using var connection = await _Dbconnection.GetConnectionAsync();
        var sql = "INSERT INTO SuperHeros (FirstName, LastName,Power,Place) VALUES (@FirstName, @LastName, @Power,@Place);";
        var affectedRows = await connection.ExecuteAsync(sql, new { superHero.FirstName, superHero.LastName, superHero.Power, superHero.Place });
        return affectedRows;
    }

    public async Task<int> DeleteHero(int ID)
    {
        await using var connection = await _Dbconnection.GetConnectionAsync();
        var sql = "DELETE FROM SuperHeros WHERE Id = @Id;";
        var affectedRows = await connection.ExecuteAsync(sql, new { ID });
        return affectedRows;
    }

    public async Task<List<UserImage>> GetImagePath(int UID)
    {
        await using var connection = await _Dbconnection.GetConnectionAsync();
        var sql = "SELECT * FROM UserImage WHERE UID = @UID;";
        var result = await connection.QueryAsync<UserImage>(sql, new { UID });
        return result.ToList();
    }



}
