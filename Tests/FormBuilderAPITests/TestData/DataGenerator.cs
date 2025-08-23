using FormBuilderAPI.Models;


namespace FormBuilderAPITests.TestData;

public class DataGenerator
{
    //First Do I create a new DBContext class or use the existing? 
    //I think I might have to put in the Fake Data call within the context 
    private readonly ApplicationDbContext _context;

    public DataGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Example uses Faker facade
    /// </summary>
    public static class FakeData
    {
        
    }
    
    
}