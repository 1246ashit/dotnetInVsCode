using Dapper;
using MyDemo.Enties;

namespace MyDemo.Services;

public interface IUserService{
    Task<int> CheckedUser(LoginEntity login);
}
public class UserService:IUserService
{
    private readonly IDbconnection _Dbconnection;
    public UserService( IDbconnection Dbconnection){
        _Dbconnection=Dbconnection;
    }

    public async Task<int> CheckedUser(LoginEntity login)
    {
        await using var connection = await _Dbconnection.GetConnectionAsync();
        // 使用參數化查詢來提高安全性
        var query = "SELECT ID FROM Users WHERE Password = @Password AND Email = @Email";
        var parameters = new { Email = login.Email, Password = login.Password };
        var ID = await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
        // 如果 count 大於 0，則表示找到了匹配的用戶
        return ID;
    }

}
