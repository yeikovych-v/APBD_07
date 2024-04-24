namespace APBD_07.Model;

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