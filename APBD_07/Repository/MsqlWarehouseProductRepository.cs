using System.Data.SqlClient;
using APBD_07.Model;

namespace APBD_07.Repository;

public class MsqlWarehouseProductRepository(IConfiguration configuration, IWarehouseRepository warehouseRepository, IProductRepository productRepository, IOrderRepository orderRepository) : IWarehouseProductRepository
{
    private List<WarehouseProduct> _wp = new();
    private static readonly int Index = 0;

    // public int InsertCompletedOrder(int warehouseId, int productId, int orderId, int amount, decimal orderPrice,
    //     DateTime createdAt)
    // {
    //     var p = new Product(productId, "", "", 10);
    //     _wp.Add(new WarehouseProduct(Index, new Warehouse(warehouseId, "", ""), p,
    //         new Order(orderId, p, amount, DateTime.Now), amount, orderPrice, createdAt));
    //     
    //     return Index;
    // }

    public bool InsertCompletedOrder(int warehouseId, int productId, int orderId, int amount, decimal orderPrice,
        DateTime createdAt)
    {
        Console.WriteLine($"WarehouseProduct:: Insert data [{warehouseId}, {productId}, {orderId}, {amount}, {orderPrice}, {createdAt}]");
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
                "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            command.Parameters.AddWithValue("@IdProduct", productId);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Price", orderPrice);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);

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

    public WarehouseProduct? FindByOrderAndProductAndWarehouseId(int orderId, int productId, int warehouseId)
    {
        Console.WriteLine($"WarehouseProduct:: Find By order, product and warehouse Id [{orderId}, {productId}, {warehouseId}]");
        
        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();
        
        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT * FROM Product_Warehouse WHERE IdProduct = @IdProduct AND IdWarehouse = @IdWarehouse AND IdOrder = @IdOrder";
        command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
        command.Parameters.AddWithValue("@IdProduct", productId);
        command.Parameters.AddWithValue("@IdOrder", orderId);
        
        using SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            var warProdId = Convert.ToInt32(reader["IdProductWarehouse"]);
            
            var warehouse = warehouseRepository.FindById(warehouseId);
            var product = productRepository.FindById(productId);
            var order = orderRepository.FindByOrderAndProductId(orderId, productId);
            if (warehouse == null || product == null || order == null) return null;
            
            var amount = Convert.ToInt32(reader["Amount"]);
            var price = Convert.ToDecimal(reader["Price"]);
            var createdAt = Convert.ToDateTime(reader["CreatedAt"]);
            
            var prodWar = new WarehouseProduct(warProdId, warehouse, product, order, amount, price, createdAt);

            return prodWar;
        }

        return null;
    }
}