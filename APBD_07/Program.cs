using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace APBD_07;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers().AddXmlSerializerFormatters();
        builder.Services.AddScoped<IOrderRepository, MsSqlOrderDb>();
        builder.Services.AddScoped<IProductRepository, MsSqlProductDb>();
        builder.Services.AddScoped<IWarehouseRepository, MsSqlWarehouseDb>();
        builder.Services.AddScoped<IWarehouseProductRepository, MsqlWarehouseProductRepository>();
        builder.Services.AddScoped<WarehouseFunctionsService>();
        builder.Services.AddScoped<ProcedureService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}

public class WarehouseFunctionsService(
    IProductRepository productRepository,
    IOrderRepository orderRepository,
    IWarehouseRepository warehouseRepository,
    IWarehouseProductRepository warehouseProductRepository)
{
    public bool ProductExists(int productId)
    {
        if (productId < 0) return false;
        return productRepository.FindById(productId) != null;
    }

    public bool WarehouseExists(int warehouseId)
    {
        if (warehouseId < 0) return false;
        return warehouseRepository.FindById(warehouseId) != null;
    }

    public bool OrderExists(int productId, int amount, DateTime timeCreated)
    {
        var order = orderRepository.FindByProductAndAmount(productId, amount);
        if (order == null) return false;

        return order.CreatedAt < timeCreated;
    }

    public Order? GetOrder(int productId, int amount)
    {
        return orderRepository.FindByProductAndAmount(productId, amount);
    }

    public void SetFulfilledTime(Order order)
    {
        order.FulfilledAt = DateTime.Now;
        orderRepository.UpdateById(order.Id, order);
    }

    public bool AddCompletedOrder(Order order, int warehouseId)
    {
        return warehouseProductRepository.InsertCompletedOrder(warehouseId, order.Product.Id, order.Id, order.Amount,
            order.Amount * order.Product.Price, DateTime.Now);
    }

    public int GetCompletedOrderId(int orderId, int productId, int warehouseId)
    {
        var warProd = warehouseProductRepository.FindByOrderAndProductAndWarehouseId(orderId, productId, warehouseId);
        if (warProd == null) throw new NullReferenceException("Warehouse Product Order was null.");
        return warProd.Id;
    }

    public bool WasOrderCompleted(Order order)
    {
        return order.FulfilledAt != DateTime.MinValue;
    }
}

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

public class Order(int id, Product product, int amount, DateTime createdAt)
{
    public int Id { get; set; } = id;
    public Product Product { get; set; } = product;
    public int Amount { get; set; } = amount;
    public DateTime CreatedAt { get; set; } = createdAt;
    public DateTime FulfilledAt { get; set; } = DateTime.MinValue;

    public override string ToString()
    {
        return $"Id: {Id}, Product: {Product}, Amount: {Amount}, CreatedAt: {CreatedAt}, FulfilledAt: {FulfilledAt}";
    }
}

public class Product(int id, string name, string desc, decimal price)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Desc { get; set; } = desc;
    public decimal Price { get; set; } = price;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Description: {Desc}, Price: {Price:C}";
    }
}

public class Warehouse(int id, string name, string address)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Address { get; set; } = address;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Address: {Address}";
    }
}

public class WarehouseProduct(
    int id,
    Warehouse warehouse,
    Product product,
    Order order,
    int amount,
    decimal price,
    DateTime createdAt)
{
    public int Id { get; set; } = id;
    public Warehouse Warehouse { get; set; } = warehouse;
    public Product Product { get; set; } = product;
    public Order Order { get; set; } = order;
    public int Amount { get; set; } = amount;
    public decimal Price { get; set; } = price;
    public DateTime CreatedAt { get; set; } = createdAt;

    public override string ToString()
    {
        return
            $"Id: {Id}, Warehouse: {Warehouse}, Product: {Product}, Order: {Order}, Amount: {Amount}, Price: {Price:C}, CreatedAt: {CreatedAt}";
    }
}

public interface IOrderRepository
{
    Order? FindByProductAndAmount(int productId, int amount);
    bool UpdateById(int id, Order order);

    public Order? FindByOrderAndProductId(int orderId, int productId);
}

public interface IProductRepository
{
    public Product? FindById(int id);
}

public interface IWarehouseProductRepository
{
    bool InsertCompletedOrder(int warehouseId, int productId, int orderId, int amount, decimal orderPrice,
        DateTime createdAt);

    WarehouseProduct? FindByOrderAndProductAndWarehouseId(int orderId, int productId, int warehouseId);
}

public interface IWarehouseRepository
{
    Warehouse? FindById(int id);
}

public class MsqlWarehouseProductRepository(
    IConfiguration configuration,
    IWarehouseRepository warehouseRepository,
    IProductRepository productRepository,
    IOrderRepository orderRepository) : IWarehouseProductRepository
{
    public bool InsertCompletedOrder(int warehouseId, int productId, int orderId, int amount, decimal orderPrice,
        DateTime createdAt)
    {
        Console.WriteLine(
            $"WarehouseProduct:: Insert data [{warehouseId}, {productId}, {orderId}, {amount}, {orderPrice}, {createdAt}]");
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
        Console.WriteLine(
            $"WarehouseProduct:: Find By order, product and warehouse Id [{orderId}, {productId}, {warehouseId}]");

        var sqlDataSource = configuration.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(sqlDataSource);

        connection.Open();

        using var command = new SqlCommand();

        command.Connection = connection;
        command.CommandText =
            "SELECT * FROM Product_Warehouse WHERE IdProduct = @IdProduct AND IdWarehouse = @IdWarehouse AND IdOrder = @IdOrder";
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

public class OrderJsonDto(int idProduct, int idWarehouse, int amount, DateTime created)
{
    public int IdProduct { get; set; } = idProduct;
    public int IdWarehouse { get; set; } = idWarehouse;
    public int Amount { get; set; } = amount;
    public DateTime Created { get; set; } = created;
}

[ApiController]
public class WarehouseController(WarehouseFunctionsService funcService, ProcedureService prodService)
{
    [HttpPost]
    [Route("api/warehouse/transaction")]
    public IResult GetProductFromWarehouseTransaction([FromBody] OrderJsonDto orderDto)
    {
        var productExists = funcService.ProductExists(orderDto.IdProduct);
        if (!productExists) return BadRequest($"Product with Id {orderDto.IdProduct} does not exist");

        var warehouseExists = funcService.WarehouseExists(orderDto.IdWarehouse);
        if (!warehouseExists) return BadRequest($"Warehouse with Id {orderDto.IdWarehouse} does not exist");

        var orderExists = funcService.OrderExists(orderDto.IdProduct, orderDto.Amount, orderDto.Created);
        if (!orderExists)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var order = funcService.GetOrder(orderDto.IdProduct, orderDto.Amount);

        if (order == null)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var wasOrderCompleted = funcService.WasOrderCompleted(order);
        if (wasOrderCompleted)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] was already completed");

        funcService.SetFulfilledTime(order);

        if (!funcService.AddCompletedOrder(order, orderDto.IdWarehouse))
            return BadRequest($"Order: [{order}] was not completed.");

        var completedOrderId = funcService.GetCompletedOrderId(order.Id, orderDto.IdProduct, orderDto.IdWarehouse);

        return Ok("Got id: " + completedOrderId);
    }

    [HttpPost]
    [Route("api/warehouse/procedure")]
    public IResult GetProductFromWarehouseProcedure([FromBody] OrderJsonDto orderDto)
    {
        var productExists = funcService.ProductExists(orderDto.IdProduct);
        if (!productExists) return BadRequest($"Product with Id {orderDto.IdProduct} does not exist");

        var warehouseExists = funcService.WarehouseExists(orderDto.IdWarehouse);
        if (!warehouseExists) return BadRequest($"Warehouse with Id {orderDto.IdWarehouse} does not exist");

        var orderExists = funcService.OrderExists(orderDto.IdProduct, orderDto.Amount, orderDto.Created);
        if (!orderExists)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var order = funcService.GetOrder(orderDto.IdProduct, orderDto.Amount);

        if (order == null)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var wasOrderCompleted = funcService.WasOrderCompleted(order);
        if (wasOrderCompleted)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] was already completed");
        try
        {
            prodService.ExecuteSetAddProcedure(orderDto.IdWarehouse, orderDto.IdProduct, orderDto.Amount);
        }
        catch (Exception e)
        {
            return BadRequest("Error occured while trying to execute procedure.");
        }

        var completedOrderId = funcService.GetCompletedOrderId(order.Id, orderDto.IdProduct, orderDto.IdWarehouse);

        return Ok("Got id: " + completedOrderId);
    }
}