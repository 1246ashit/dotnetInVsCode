using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using MyDemo.Enties;

namespace MyDemo.Services
{
    public static class JwtStartup
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtOptions = new JwtOptions();
        config.GetSection(JwtOptions.SectionName).Bind(jwtOptions); // 將設定檔中的設定值綁定到 JwtOptions 物件上

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ClockSkew = TimeSpan.Zero, // 有效時間接受的誤差
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SignKey)), // 發行者金鑰
                    ValidateAudience = false, // 驗證受眾
                    ValidateIssuer = true, // 驗證發行者
                    ValidateIssuerSigningKey = true, // 驗證發行者金鑰
                    ValidateLifetime = true, // 驗證有效時間
                    ValidIssuer = jwtOptions.Issuer, // 發行者
                };
            });

        // .AddJwtBearer();
        services.AddAuthorization();

        // 未通過身份驗證的請求自動回傳 401 Unauthorized (除非有使用 AllowAnonymousAttribute)
        services.AddControllers(options => options.Filters.Add(new AuthorizeFilter()));

        return services;
    }
}
}