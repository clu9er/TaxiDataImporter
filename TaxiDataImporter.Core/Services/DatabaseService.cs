using System.Data;
using System.Data.SqlClient;
using TaxiDataImporter.Core.Models;

namespace TaxiDataImporter.Core.Services;

public class DatabaseService
{
    private readonly string _connectionString;
    
    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task InsertBatchIntoDatabase(List<TaxiTrip> batch)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("PickupDateTimeUtc", typeof(DateTime));
        dataTable.Columns.Add("DropOffDateTimeUtc", typeof(DateTime));
        dataTable.Columns.Add("PassengerCount", typeof(int));
        dataTable.Columns.Add("TripDistance", typeof(double));
        dataTable.Columns.Add("StoreAndFwdFlag", typeof(string));
        dataTable.Columns.Add("PULocationID", typeof(int));
        dataTable.Columns.Add("DOLocationID", typeof(int));
        dataTable.Columns.Add("FareAmount", typeof(decimal));
        dataTable.Columns.Add("TipAmount", typeof(decimal));

        foreach (var trip in batch)
        {
            dataTable.Rows.Add(
                trip.PickupDateTime,
                trip.DropOffDateTime,
                trip.PassengerCount,
                trip.TripDistance,
                trip.StoreAndFwdFlag,
                trip.PULocationID,
                trip.DOLocationID,
                trip.FareAmount,
                trip.TipAmount
            );
        }

        using var bulkCopy = new SqlBulkCopy(_connectionString);
        bulkCopy.DestinationTableName = "TaxiTrips";
        await bulkCopy.WriteToServerAsync(dataTable);
    }
    
    public async Task<int> GetCountOfRowsInTable()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var commandText = "SELECT COUNT(*) FROM TaxiTrips";
        await using var command = new SqlCommand(commandText, connection);
    
        var result = await command.ExecuteScalarAsync();
    
        return Convert.ToInt32(result);
    }
}