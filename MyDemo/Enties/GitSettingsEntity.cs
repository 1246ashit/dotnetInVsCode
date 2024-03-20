namespace MyDemo.Enties;

public class GitSettingsEntity
{
    public string Token { get; set; }= null!;
    public string BareURL { get; set; }= null!;
    public string UploadUrl { get; set; }= null!;
    public string User { get; set; }= null!;
    public string Repo { get; set; }= null!;
    
}

public class FileResult
{
    public Stream FileStream { get; set; }= null!;
    public string Sha { get; set; }= null!;
}

public class UserImage{
    public string ImgPath { get; set; }= null!;
    public DateTime CreateDate{ get; set; }
}

