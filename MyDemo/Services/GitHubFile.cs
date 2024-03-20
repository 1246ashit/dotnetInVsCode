using System.Text.Json;
using MyDemo.Enties;
using RestSharp;

namespace MyDemo.Services;


public interface IGitHubFile
{
    Task<string> UploadFile(byte[] imageBytes, string fileName);

    Task<FileResult> GetFile(string fileName);
    Task DeleteFile(string imagepath, string sha);
}
public class GitHubFile : IGitHubFile
{
    private readonly RestClient _clientPostAndDelete;
    private readonly RestClient _clientGet;
    private readonly GitSettingsEntity _gitHubSettings;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // 創建一個初始計數和最大計數都為1的SemaphoreSlim

    public GitHubFile(IConfiguration configuration, GitSettingsEntity gitHubSettings)
    {
        _gitHubSettings = gitHubSettings;
        _clientPostAndDelete = new RestClient(_gitHubSettings.UploadUrl);
        _clientGet = new RestClient($"{_gitHubSettings.BareURL}/{_gitHubSettings.User}/{_gitHubSettings.Repo}/main/");
    }
    public async Task<FileResult> GetFile(string fileName)
    {
        var request = new RestRequest($"/repos/{_gitHubSettings.User}/{_gitHubSettings.Repo}/contents/{fileName}", Method.Get);
        request.AddHeader("Authorization", $"Token {_gitHubSettings.Token}");
        request.AddHeader("Accept", "application/vnd.github.v3+json");
        var response = await _clientPostAndDelete.ExecuteGetAsync(request);
        if (response.IsSuccessful)
        {
            // 解析響應以獲取 SHA 值
            string content = response.Content!.ToString();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;
            var sha = root.GetProperty("sha").GetString();

            // GitHub API 返回的是文件的 Base64 編碼內容，因此需要先解碼
            var fileContent = System.Convert.FromBase64String((string)root.GetProperty("content").GetString()!);
            var stream = new MemoryStream(fileContent);

            return new FileResult
            {
                FileStream = stream,
                Sha = sha!
            };
        }
        return null!;
    }


    public async Task<string> UploadFile(byte[] imageBytes, string fileName)
    {
        var request = new RestRequest($"/repos/{_gitHubSettings.User}/{_gitHubSettings.Repo}/contents/{fileName}", Method.Put);
        request.AddHeader("Authorization", $"Bearer {_gitHubSettings.Token}");
        //request.AddHeader("Content-Type", "application/json");
        var body = new
        {
            message = "Upload image", // 提交信息
            content = Convert.ToBase64String(imageBytes), // 圖片內容的 base64 編碼
        };
        request.AddJsonBody(body);
        var response = await _clientPostAndDelete.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            throw new ApplicationException($"GitHub API error: {response.StatusCode} {response.Content}");
        }
        //return response.Content;
        //var jsonResponse = JObject.Parse(response.Content);
        //var downloadUrl = jsonResponse["content"]["download_url"].ToString();

        return fileName; // 回傳圖片的下載 URL
    }

    public async Task DeleteFile(string imagepath, string sha)
    {
        await _semaphore.WaitAsync(); // 等待獲得鎖
        try
        {
            var request = new RestRequest($"/repos/{_gitHubSettings.User}/{_gitHubSettings.Repo}/contents/{imagepath}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {_gitHubSettings.Token}");
            request.AddHeader("Accept", "application/vnd.github+json");
            request.AddHeader("X-GitHub-Api-Version", "2022-11-28");
            // GitHub 需要提交變動的訊息，這對於刪除操作也是必須的
            request.AddJsonBody(new
            {
                message = $"Delete {imagepath}",
                committer = new { name = _gitHubSettings.User, email = "1246ashit@gmail.com" },
                sha = sha
            });

            var response = await _clientPostAndDelete.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to delete file: {response.ErrorMessage}");
            }
        }
        finally
        {
            _semaphore.Release(); // 釋放鎖
        }
    }

}
