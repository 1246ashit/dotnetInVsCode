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
    private readonly HttpClient _httpClient;
    private readonly GitSettingsEntity _gitHubSettings;
    public HomeController(GitSettingsEntity gitHubSettings,HttpClient httpClient,IUserService userService, JwtService jwtService, IMySqlService MySqlService, CSRedisClient rds)
    {
        _MySqlService = MySqlService;
        _jwtService = jwtService;
        _rds = rds;
        _userService = userService;
        _httpClient = httpClient;
        _gitHubSettings=gitHubSettings;
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

    [AllowAnonymous]
    [HttpGet("GetAll")]
    public async Task<ActionResult<List<SuperHeroEntity>>> GetAll()
    {
        var heros = await _MySqlService.GetData();

        return Ok(heros);
    }
    [AllowAnonymous]
    [HttpPost("CeateNewHero")]
    public async Task<ActionResult<List<SuperHeroEntity>>> CeateNewHero(SuperHeroEntity superHero)
    {
        var affectedRows = await _MySqlService.AddHero(superHero);
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
        var affectedRows = await _MySqlService.DeleteHero(heroId.Id);
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
        var name = _rds.Get("name");
        return Ok(name);
    
    }
    
    [AllowAnonymous]
    [HttpGet("GetImage")]
    public async Task<IActionResult> GetImage(string imagePath)
    {
        string token = _gitHubSettings.Token;//GitHub token
        string baseUrl = _gitHubSettings.BareURL;
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
            
        var response = await _httpClient.GetAsync(baseUrl + imagePath);
        if (response.IsSuccessStatusCode)
        {
            var imageStream = await response.Content.ReadAsStreamAsync();
            return File(imageStream, "image/jpeg"); //根據實際圖片類型調整MIME類型
        }
        return NotFound();
    
    }

}
