using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class UsersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ReturnsSeededUsers()
    {
        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/users");
        Assert.NotNull(users);
        Assert.True(users!.Count >= 3);
    }

    [Fact]
    public async Task CreateUser_ReturnsSuccessStatusCode()
    {
        var response = await _client.PostAsJsonAsync("/users", new CreateUserRequest("Katherine", "Johnson", "kj@example.com"));
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DeleteUnknownUser_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Pagination_ReturnsSubset()
    {
        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/users?page=1&pageSize=2");
        Assert.Equal(2, users!.Count);
    }

    [Fact]
    public void FormatDisplayName_UsesPreferredOrder_WhenRequested()
    {
        var service = new UserService();
        var formatted = service.FormatDisplayName(new User(4, "Grace", "Hopper", "grace@example.com", true));
        Assert.Equal("Hopper, Grace", formatted);
    }
}
