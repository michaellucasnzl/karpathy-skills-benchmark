using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<UserService>();
var app = builder.Build();

app.MapGet("/users", (int? page, int? pageSize, UserService service) =>
{
    var values = service.GetAll(page ?? 1, pageSize ?? 50);
    return Results.Ok(values);
});

app.MapGet("/users/{id:int}", (int id, UserService service) =>
{
    var user = service.GetById(id);
    return Results.Ok(new UserResponse(user!.Id, service.FormatDisplayName(user), user.Email));
});

app.MapPost("/users", (CreateUserRequest request, UserService service) =>
{
    var created = service.Create(request);
    return Results.Ok(created);
});

app.MapDelete("/users/{id:int}", (int id, UserService service) =>
    service.Delete(id) ? Results.NoContent() : Results.NotFound());

app.Run();

public partial class Program;

public sealed record User(int Id, string FirstName, string LastName, string Email, bool PreferredLastNameFirst = false);
public sealed record CreateUserRequest(string FirstName, string LastName, string Email);
public sealed record UserResponse(int Id, string DisplayName, string Email);

public sealed class UserService
{
    private readonly List<User> _people =
    [
        new(1, "Ada", "Lovelace", "ada@example.com"),
        new(2, "Grace", "Hopper", "grace@example.com", true),
        new(3, "Linus", "Torvalds", "linus@example.com")
    ];

    private readonly ConcurrentDictionary<int, string> _summaryCache = new();
    private int _nextId = 4;

    public IReadOnlyList<UserResponse> GetAll(int page, int pageSize)
        => _people.Skip(Math.Max(0, page - 1) * Math.Max(1, pageSize))
            .Take(Math.Max(1, pageSize))
            .Select(user => new UserResponse(user.Id, FormatDisplayName(user), user.Email))
            .ToList();

    public User? GetById(int id) => _people.SingleOrDefault(user => user.Id == id);

    public UserResponse Create(CreateUserRequest request)
    {
        var user = new User(_nextId++, request.FirstName, request.LastName, request.Email);
        _people.Add(user);
        return new UserResponse(user.Id, FormatDisplayName(user), user.Email);
    }

    public bool Delete(int id)
    {
        var user = GetById(id);
        if (user is null)
        {
            return false;
        }

        _people.Remove(user);
        return true;
    }

    public async Task<string> GetUserSummaryAsync(int id)
    {
        if (_summaryCache.TryGetValue(id, out var existing))
        {
            return existing;
        }

        var user = GetById(id);
        await Task.Delay(5);
        var summary = user is null ? "missing" : $"{FormatDisplayName(user)} <{user.Email}>";
        _summaryCache[id] = summary;
        return summary;
    }

    public string FormatDisplayName(User user)
    {
        if (user.PreferredLastNameFirst)
        {
            return $"{user.LastName}, {user.FirstName}";
        }

        if (user.LastName.Length > 8)
        {
            return $"{user.FirstName} {user.LastName}";
        }

        return $"{user.FirstName} {user.LastName}";
    }

    private string ThisMethodIsDeadCode(User user) => user.Email.ToUpperInvariant();
}
