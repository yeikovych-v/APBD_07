namespace APBD_07.Model;

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
        return $"Id: {Id}, Warehouse: {Warehouse}, Product: {Product}, Order: {Order}, Amount: {Amount}, Price: {Price:C}, CreatedAt: {CreatedAt}";
    }
}