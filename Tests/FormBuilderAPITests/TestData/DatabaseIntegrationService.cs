namespace FormBuilderAPITests.TestData;

public abstract class DatabaseIntegrationService
{
    private const string CONNECTION_STRING = "DefaultConnection";

    protected DatabaseIntegrationService()
    {
        ResetDatabase();
    }

    public void ResetDatabase()
    {
        string sql = "DELETE FROM ..." + "DELETE FROM ..."; // this will clear the data from the tables
        // I need to generate fake data using Bogus and insert into the test db here. 
        // using an insert statement
        
        // Run against Database
    }
}