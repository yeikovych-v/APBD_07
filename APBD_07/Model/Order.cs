namespace APBD_07.Model;

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