using System.Data.SqlClient;
using APBD_07.Model;

namespace APBD_07.Repository;

public class MsSqlWarehouseDb(IConfiguration configuration) : IWarehouseRepository
{   
    
    public Warehouse? FindById(int id)
    {
        Console.WriteLine($"Warehouse:: Find By Id [{id}]");
        
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();
        
        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @Id";
        command.Parameters.AddWithValue("@Id", id);
        
        using SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var warId = Convert.ToInt32(reader["IdWarehouse"]);
            var name = reader["Name"].ToString() ?? "";
            var address = reader["Address"].ToString() ?? "";

            var warehouse = new Warehouse(warId, name, address);

            return warehouse;
        }

        return null;
    }
}