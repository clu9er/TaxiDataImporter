using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using TaxiDataImporter.Core.Extensions;
using TaxiDataImporter.Core.Models;

namespace TaxiDataImporter.Core.Services;

public class CsvService
{
    private readonly string _filePath;
    private readonly string _duplicatesFilePath;
    private readonly DatabaseService _databaseService;
    
    public CsvService(string filePath, string duplicatesFilePath, DatabaseService databaseService)
    {
        _filePath = filePath;
        _duplicatesFilePath = duplicatesFilePath;
        _databaseService = databaseService;
    }
    
    public async Task ProcessAndInsertTaxiTrips()
    {
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," };
        const int batchSize = 10000;
        var uniqueTrips = new HashSet<(DateTime?, DateTime?, int?)>();
        var batch = new List<TaxiTrip>(batchSize);
        var duplicateTrips = new List<TaxiTrip>();

        using (var reader = new StreamReader(_filePath))
        using (var csvReader = new CsvReader(reader, configuration))
        {
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            while (await csvReader.ReadAsync())
            {
                var trip = csvReader.GetRecord<TaxiTrip>();
                var tripKey = (PickupDateTimeUtc: trip.PickupDateTime, DropOffDateTimeUtc: trip.DropOffDateTime, trip.PassengerCount);

                trip.StoreAndFwdFlag = trip.StoreAndFwdFlag.Trim();
                trip.PickupDateTime = trip.PickupDateTime.ConvertToUtc();
                trip.DropOffDateTime = trip.DropOffDateTime.ConvertToUtc();

                if (!uniqueTrips.Add(tripKey) || trip.PickupDateTime == null || trip.DropOffDateTime == null || trip.PassengerCount == null)
                {
                    duplicateTrips.Add(trip);
                    continue;
                }

                trip.StoreAndFwdFlag = trip.StoreAndFwdFlag.Equals("y", StringComparison.CurrentCultureIgnoreCase) ? "Yes" : "No";

                batch.Add(trip);

                if (batch.Count < batchSize) continue;

                await _databaseService.InsertBatchIntoDatabase(batch);
                batch.Clear();
            }

            if (batch.Count > 0)
            {
                await _databaseService.InsertBatchIntoDatabase(batch);
            }
        }

        await WriteDuplicatesToFile(duplicateTrips);
    }
    
    private async Task WriteDuplicatesToFile(IEnumerable<TaxiTrip> duplicateTrips)
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;

        var filePath = Path.Combine(projectDirectory!, _duplicatesFilePath);    

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        // Create or overwrite the file
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    
        var batchSize = 1000;
        var batch = new List<TaxiTrip>(batchSize);

        foreach (var trip in duplicateTrips)
        {
            batch.Add(trip);
            if (batch.Count < batchSize) continue;
            await csv.WriteRecordsAsync(batch);
            batch.Clear();
        }

        // Write any remaining records
        if (batch.Count > 0)
        {
            await csv.WriteRecordsAsync(batch);
        }
    }
}