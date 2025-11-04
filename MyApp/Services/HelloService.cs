namespace MyApp.Services;

public class HelloService
{
    public string GetGreeting(string name = "World")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "World";
        }
        return $"Hello {name}";
    }
}

