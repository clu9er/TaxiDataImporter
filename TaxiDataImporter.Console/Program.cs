using Microsoft.Extensions.Configuration;
using TaxiDataImporter.Core.Services;

namespace TaxiDataImporter.Console;

static class Program
{
    private static IConfiguration _configuration;
    
    static async Task Main(string[] args)
    {
        LoadConfiguration();

        System.Console.WriteLine("Taxi Data Importer");

        try
        {
            var databaseService = new DatabaseService(_configuration.GetConnectionString("DefaultConnection")!);
            var csvService = new CsvService(_configuration["CsvFilePath"]!, _configuration["DuplicatesFilePath"]!, databaseService);

            await csvService.ProcessAndInsertTaxiTrips();
            
            var count = await databaseService.GetCountOfRowsInTable();
            
            System.Console.WriteLine("Data import completed successfully. Rows inserted: " + count);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void LoadConfiguration()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}