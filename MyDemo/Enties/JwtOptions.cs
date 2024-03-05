using System.ComponentModel.DataAnnotations;

namespace MyDemo.Enties
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required] public string SignKey { get; set; } = null!;
        [Required] public string Issuer { get; set; } = null!;
        public int ExpireMinutes { get; set; } = 60 * 24; // 過期時間(分鐘)，這裡範例的是一天(24 小時)
    }
}