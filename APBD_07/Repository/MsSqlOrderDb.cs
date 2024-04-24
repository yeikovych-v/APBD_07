using System.Data.SqlClient;
using APBD_07.Model;

namespace APBD_07.Repository;

public class MsSqlOrderDb(IConfiguration configuration, IProductRepository productRepository) : IOrderRepository
{

    public Order? FindByProductAndAmount(int productId, int amount)
    {
        Console.WriteLine($"Order:: Find By ProductId [{productId}] && Amount [{amount}]");

        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();

        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT * FROM Order WHERE IdProduct = @Id AND Amount = @Amount";
        command.Parameters.AddWithValue("@Id", productId);
        command.Parameters.AddWithValue("@Amount", amount);

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            var orderId = Convert.ToInt32(reader["IdOrder"]);
            
            var product = productRepository.FindById(productId);
            if (product == null) return null;
            
            var createdAt = Convert.ToDateTime(reader["CreatedAt"]);
            
            var fulfilledAt = DateTime.MinValue;
            if (reader["FulfilledAt"] != DBNull.Value)
            {
                fulfilledAt = Convert.ToDateTime(reader["FulfilledAt"]);
            }
            
            var order = new Order(orderId, product, amount, createdAt)
            {
                FulfilledAt = fulfilledAt
            };

            return order;
        }

        return null;
    }

    public bool UpdateById(int id, Order order)
    {
        Console.WriteLine($"Order:: Update by id [{id}]");
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();
        
        using var command = new SqlCommand();
        SqlTransaction sqlTransaction = connection.BeginTransaction();
        
        command.Connection = connection;
        command.Transaction = sqlTransaction;
        try
        {
            command.CommandText =
                "UPDATE Order SET IdProduct = @IdProduct, Amount = @Amount, CreatedAt = @CreatedAt, FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdProduct", order.Product.Id);
            command.Parameters.AddWithValue("@Amount", order.Amount);
            command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
            command.Parameters.AddWithValue("@FulfilledAt", order.FulfilledAt);
            command.Parameters.AddWithValue("@IdOrder", id);

            command.ExecuteNonQuery();

            sqlTransaction.Commit();
            return true;
        }
        catch (Exception e)
        {
            sqlTransaction.Rollback();
            return false;
        }
    }
    
    public Order? FindByOrderAndProductId(int orderId, int productId)
    {
        Console.WriteLine($"Order:: Find By Id [{orderId}]");
        
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();
        
        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT * FROM Order WHERE IdOrder = @Id";
        command.Parameters.AddWithValue("@Id", orderId);
        
        using SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var product = productRepository.FindById(productId);
            if (product == null) return null;

            var amount = Convert.ToInt32(reader["Amount"]);
            
            var createdAt = Convert.ToDateTime(reader["CreatedAt"]);
            
            var fulfilledAt = DateTime.MinValue;
            if (reader["FulfilledAt"] != DBNull.Value)
            {
                fulfilledAt = Convert.ToDateTime(reader["FulfilledAt"]);
            }
            
            var order = new Order(orderId, product, amount, createdAt)
            {
                FulfilledAt = fulfilledAt
            };
            
            return order;
        }

        return null;
    }
}