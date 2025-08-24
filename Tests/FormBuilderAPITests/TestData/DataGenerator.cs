using FormBuilderAPI.Models;
using Bogus;
using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;



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
        public static List<Form> Forms = new List<Form>();
        public static List<Domain> Domains = new List<Domain>();
        // Do I need the Forms_Domain here? 
        
        public static void Init(int count)
        {
            var domainId = 1;
            var domainFaker = new Faker<Domain>()
                .RuleFor(x => x.Id, f => domainId++)
                .RuleFor(x => x.Type, f => f.Hacker.Noun())
                .RuleFor(x => x.CreatedDate, f => f.Date.Past())
                .RuleFor(x => x.LastModifiedDate, f => f.Date.Past());
            
            
        }
    }
    
    
}

