using MyDemo.Enties;
using RestSharp;

namespace MyDemo.Services;


public interface IGitHubFile
{
    Task<string> UploadFile(byte[] imageBytes, string fileName);

    Task<Stream> GetFile(string fileName);
}
public class GitHubFile : IGitHubFile
{
    private readonly RestClient _clientPost;
    private readonly RestClient _clientGet;
    private readonly GitSettingsEntity _gitHubSettings;

    public GitHubFile(IConfiguration configuration, GitSettingsEntity gitHubSettings)
    {
        _gitHubSettings = gitHubSettings;
        _clientPost = new RestClient(_gitHubSettings.UploadUrl);
        _clientGet = new RestClient($"https://raw.githubusercontent.com/1246ashit/newCSONline/main/");
    }
    public async Task<Stream> GetFile(string fileName)
    {
        var request = new RestRequest(fileName, Method.Get);
        request.AddHeader("Authorization", $"Token {_gitHubSettings.Token}");
        var response = await _clientGet.ExecuteGetAsync(request);
        if (response.IsSuccessful && response.ContentLength > 0)
        {
            var stream = new MemoryStream(response.RawBytes);
            return stream;
        }
        return null;
    }


    public async Task<string> UploadFile(byte[] imageBytes, string fileName)
    {
        var request = new RestRequest($"/repos/{_gitHubSettings.User}/{_gitHubSettings.Repo}/contents/{fileName}", Method.Put);
        request.AddHeader("Authorization", $"Bearer {_gitHubSettings.Token}");
        request.AddHeader("Content-Type", "application/json");

        var body = new
        {
            message = "Upload image", // 提交信息
            content = Convert.ToBase64String(imageBytes), // 圖片內容的 base64 編碼
        };
        request.AddJsonBody(body);
        var response = await _clientPost.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            throw new ApplicationException($"GitHub API error: {response.StatusCode} {response.Content}");
        }
        //return response.Content;
        //var jsonResponse = JObject.Parse(response.Content);
        //var downloadUrl = jsonResponse["content"]["download_url"].ToString();

        return fileName; // 回傳圖片的下載 URL

    }

}
