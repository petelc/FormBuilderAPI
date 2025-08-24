using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit.Abstractions;
using FormBuilderAPI.DTO;

namespace FormBuilderAPITests;

public class FormTests(ITestOutputHelper output, WebApplicationFactory<Program> application)
    : IClassFixture<WebApplicationFactory<Program>>
{
    readonly HttpClient _client = application.CreateClient();


    [Fact]
    public async Task ShouldGetAllForms()
    {
        output.WriteLine("Fetching forms from database...");
        

        var response = await _client.GetAsync("https://localhost:7213/api/Form");
        response.EnsureSuccessStatusCode();
        output.WriteLine("Done fetching forms from database");
        output.WriteLine(await response.Content.ReadAsStringAsync());
        //response.StatusCode.Should().Be(HttpStatusCode.OK);
        // var expectedResult = HttpStatusCode.OK;
        // Assert.NotEmpty(response.Content!);

    }
}