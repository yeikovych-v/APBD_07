using System.Data;
using System.Data.SqlClient;
using APBD_07.Model;

namespace APBD_07.Service;

public class ProcedureService(IConfiguration configuration)
{
    public void ExecuteSetAddProcedure(int warehouseId, int productId, int amount)
    {
        Console.WriteLine($"ProcedureService:: Inside Set Add Procedure");
        
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();

        using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        command.ExecuteNonQuery();
    }
}