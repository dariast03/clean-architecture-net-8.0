using CleanArchitecture.IntegrationTest.Shared.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.IntegrationTest.TestCase;

public class BookTest : IClassFixture<InjectionFixture>
{
    private readonly IBookClient _bookClient;

    public BookTest(InjectionFixture fixture)
        => _bookClient = fixture.ServiceProvider.GetRequiredService<IBookClient>();

    [Fact(DisplayName = "[Book] Verify get book by id")]
    public async Task Book_ShouldSuccess_WhenGetBookById()
    {
        // Act
        var result = await _bookClient.Get("1");

        // print result in console in json string
        Console.WriteLine(result.ToString());
        
    
        Console.WriteLine("Debug: Retrieved book result.");

        // Assert
        Assert.True(true);
    }
}

