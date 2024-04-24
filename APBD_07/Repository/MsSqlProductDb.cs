using System.Data.SqlClient;
using APBD_07.Model;

namespace APBD_07.Repository;

public class MsSqlProductDb(IConfiguration configuration) : IProductRepository
{
    public Product? FindById(int id)
    {
        Console.WriteLine($"Product:: Find By Id [{id}]");
        
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();
        
        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT * FROM Product WHERE IdProduct = @Id";
        command.Parameters.AddWithValue("@Id", id);
        
        using SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var pId = Convert.ToInt32(reader["IdProduct"]);
            var name = reader["Name"].ToString() ?? "";
            var desc = reader["Description"].ToString() ?? "";
            var price = Convert.ToDecimal(reader["Price"]);

            var product = new Product(pId, name, desc, price);

            return product;
        }

        return null;
    }
}