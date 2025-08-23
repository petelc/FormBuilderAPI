using FormBuilderAPI.Controllers;
using Xunit.Abstractions;

namespace FormBuilderAPITests;

public class AuthTests : IDisposable
{
    private AccountController  _sut;
    private readonly ITestOutputHelper _output;

    public AuthTests(ITestOutputHelper output)
    {
        
        _output = output;
        _output.WriteLine("In the constructor of AuthTests class");
    }
    
    [Fact]
    public void UserShouldBeAbleToLogin()
    {
        //Arrange
        
        //Act
        
        //Assert
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
