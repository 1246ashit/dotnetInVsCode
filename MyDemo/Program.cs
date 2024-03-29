using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyDemo.Services;
using MyDemo.Enties;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var _config = builder.Configuration;

//cros
var myReactAppOrigins = "_myReactAppOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myReactAppOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .WithExposedHeaders("X-Image-SHA");
                      });
});
//

//jwt
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<JwtService>();
builder.Services.AddJwtAuthentication(builder.Configuration);
//

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    //let swager can put JwtToken 
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    //
});

//redis
var rds = new CSRedisClient("localhost:6379");
RedisHelper.Initialization(rds);
builder.Services.AddSingleton<CSRedisClient>(rds);
//

// 依賴注入
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDbconnection,Dbconnection>();
builder.Services.AddScoped<IMySqlService,MySqlService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IGitHubFile,GitHubFile>();
///github圖庫設定
var gitHubSettings = builder.Configuration.GetSection("GitHubSettings").Get<GitSettingsEntity>()?? throw new InvalidOperationException("GitHub settings must be configured.");
builder.Services.AddSingleton(gitHubSettings);
//

// 控制器
builder.Services.AddControllers();
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(myReactAppOrigins);

//jwt
app.UseAuthentication(); // 身份驗證
app.UseAuthorization(); // 驗證授權 (原本應該就有這行，注意不要重複加入)
//
app.MapControllers(); // 這裡配置路由以使用控制器




app.Run();

