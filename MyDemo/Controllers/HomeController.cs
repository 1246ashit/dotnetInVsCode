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
    private readonly JwtService _jwtService;
    private readonly CSRedisClient _rds;
    private readonly IMySqlService _mySqlService;
    private readonly IUserService _userService;
    public HomeController(  IUserService userService, 
                            JwtService jwtService, IMySqlService mySqlService, CSRedisClient rds)
    {
        _mySqlService = mySqlService;
        _jwtService = jwtService;
        _rds = rds;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult> Login([FromBody] LoginEntity login)
    {
        var id = await _userService.CheckedUser(login);
        if (id!=0)
        {
            var token = _jwtService.GenerateToken(login.Email); // 依照帳號產生 jwt
            return Ok(new{id=id,token=token});
        }
        else
        {
            return BadRequest("查無此人");
        }
    }

    [AllowAnonymous]
    [HttpGet("GetAll")]
    public async Task<ActionResult<List<SuperHeroEntity>>> GetAll()
    {
        var heros = await _mySqlService.GetData();

        return Ok(heros);
    }
    [AllowAnonymous]
    [HttpPost("CeateNewHero")]
    public async Task<ActionResult<List<SuperHeroEntity>>> CeateNewHero(SuperHeroEntity superHero)
    {
        var affectedRows = await _mySqlService.AddHero(superHero);
        if (affectedRows == 1)
        {

            return Ok();
        }
        else
        {
            return BadRequest("無效操作的描述");
        }
    }


    [AllowAnonymous]
    [HttpPost("DeleteHero")]
    public async Task<ActionResult> DeleteHero([FromBody] HeroId heroId)
    {
        var affectedRows = await _mySqlService.DeleteHero(heroId.Id);
        if (affectedRows == 1)
        {

            return Ok(new { message = "success" });
        }
        else
        {
            return BadRequest(new { error = "無效操作的描述" });
        }
    }

    [AllowAnonymous]
    [HttpGet("GetfromRedis")]
    public async Task<IActionResult> GetfromRedis()
    {
        // 從 jwt 取得使用者名稱
        _rds.Set("name", "臭雞雞");
        var name = await _rds.GetAsync("name");
        return Ok(name);

    }


}


