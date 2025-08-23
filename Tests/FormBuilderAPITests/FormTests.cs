using FormBuilderAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace FormBuilderAPITests;

public class FormTests
{
    private readonly ApplicationDbContext _db;
    private readonly ITestOutputHelper _output;

    public FormTests(ITestOutputHelper output)
    {
        _output = output;
        DbContextOptionsBuilder<ApplicationDbContext> dbOptions = new();
        dbOptions.UseSqlite("Filename=../../../FormBuilderAPI/formBuilder.db");
        _db = new ApplicationDbContext(dbOptions.Options);
    }
    
    
    [Fact]
    public void ShouldGetAllForms()
    {
        _output.WriteLine("Fetching forms from database...");
        // 1. Use the _db to connect to database and query the Forms table
        // 2. Create a list of type form to store results in. 
        // 3. Test if the list has data and that the count is 4
        
        
        List<Form> forms = _db.Forms.ToList();
        foreach (var form in forms)
        {
            //Check the data that has come back (DEBUGGING)
            _output.WriteLine(form.ToString());
        }
        
        Assert.NotNull(forms);
        Assert.NotEmpty(forms);
        Assert.True(forms.Count > 0);
        
    }
}