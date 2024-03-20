
using Microsoft.AspNetCore.Mvc;
using MyDemo.Services;
using MyDemo.Enties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MyDemo.Controllers;
[Route("api/[controller]")]
public class ImageCRUDController : ControllerBase
{
    private readonly IGitHubFile _gitHubFile;
    private readonly GitSettingsEntity _gitHubSettings;
    private readonly IMySqlService _mySqlService;
    public ImageCRUDController(IMySqlService mySqlService,
                                IGitHubFile gitHubFile,GitSettingsEntity gitHubSettings){
        _mySqlService = mySqlService;
        _gitHubSettings = gitHubSettings;
        _gitHubFile=gitHubFile;
    }
    [AllowAnonymous]
    [HttpGet("GetImgPath")]
    public async Task<IActionResult> GetImgPath(int UID){
        var result=await _mySqlService.GetImagePath(UID);
        return Ok(result);
    }


    [AllowAnonymous]
    [HttpPost("PostImage")]
    public async Task<IActionResult> PostImage(IFormFile file){
        if (file.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var response = await _gitHubFile.UploadFile(content, file.FileName);
                return Ok(response);
            }
        }
        return BadRequest("No file received");
    }

    [AllowAnonymous]
    [HttpGet("GetImage")]
    public async Task<IActionResult> GetImage(string imagePath)
    {
        var imageStream = await _gitHubFile.GetFile(imagePath);
        if (imageStream != null)
        {
            Response.Headers.Append("X-Image-SHA", imageStream.Sha);
            return File(imageStream.FileStream, "image/jpeg"); // 根據實際圖片類型調整 MIME 類型
        }
        return NotFound();
    }

    [AllowAnonymous]
    [HttpDelete("DeleteImage")]
    public async Task<IActionResult> DeleteImage(string path,string sha)
    {
        try
        {
            await _gitHubFile.DeleteFile(path,sha);
            return Ok("File deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error deleting file: {ex.Message}");
        }
    }



}
