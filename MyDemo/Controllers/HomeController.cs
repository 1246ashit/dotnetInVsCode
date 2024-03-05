using System.Data.SqlClient;
using CSRedis;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDemo.Enties;
using MyDemo.Services;

namespace MyDemo.Controllers;


[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IMySqlService _MySqlService;
    private readonly JwtService _jwtService;
    private readonly CSRedisClient _rds;
    private readonly IUserService _userService;
    public HomeController(IUserService userService,JwtService jwtService,IMySqlService MySqlService,CSRedisClient rds){
        _MySqlService = MySqlService;
        _jwtService = jwtService;
        _rds=rds;
        _userService=userService;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] LoginEntity login)
    {
        var userExists = await _userService.CheckedUser(login);
        if (userExists)
        {
            var token = _jwtService.GenerateToken(login.Email); // 依照帳號產生 jwt
            return Ok(token);
        }
        else
        {
            return BadRequest("查無此人");
        }
    }


    [HttpGet("GetAll")]
    public async Task<ActionResult<List<SuperHeroEntity>>>GetAll(){
        var heros=await _MySqlService.GetData();

        return Ok(heros);
    }
    [AllowAnonymous]
    [HttpPost("CeateNewHero")]
    public async Task<ActionResult<List<SuperHeroEntity>>>CeateNewHero([FromBody]SuperHeroEntity superHero){
        var affectedRows=await _MySqlService.AddHero(superHero);
        if (affectedRows==1){
            
            return Ok();
        }
        else{
            return BadRequest("無效操作的描述");
        }
    }


    public record class TokenDto(string Token);
    [AllowAnonymous] // 允許匿名登入
    [HttpPost("LoginTokenDto")]
    public ActionResult<string> LoginTokenDto([FromBody] LoginEntity login)
    {
        // todo: 驗證帳號密碼

        var token = _jwtService.GenerateToken(login.Email); // 依照帳號產生 jwt

        return Ok(new TokenDto(token));
    }

    [HttpGet("Profile")]
    public ActionResult<string> Profile()
    {
        // 從 jwt 取得使用者名稱
        var userName = User.Identity?.Name;
        return Ok(userName);
    }

    [AllowAnonymous]
    [HttpGet("GetfromRedis")]
    public ActionResult<string> GetfromRedis()
    {
        // 從 jwt 取得使用者名稱
        _rds.Set("name", "臭雞雞");
        var name=_rds.Get("name");
        return Ok(name);
    }
}
