namespace MyDemo.Enties;

public class SuperHeroEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Power { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
}

public class HeroId
{
    public int Id { get; set; }
}
