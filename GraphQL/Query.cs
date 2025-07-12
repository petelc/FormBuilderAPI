using FormBuilderAPI.Models;

namespace FormBuilderAPI.GraphQL;

public class Query
{
    [Serial]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [GraphQLType(typeof(MyDictionaryType))]
    public IQueryable<Form> GetForms([Service] ApplicationDbContext context) => context.Forms;

    [GraphQLType(typeof(MyDictionaryType))]
    public MyCustomDictionary GetSampleDictionary()
    {
        var dict = new MyCustomDictionary();
        dict["foo"] = "bar";
        dict["number"] = 42;
        return dict;
    }
}
