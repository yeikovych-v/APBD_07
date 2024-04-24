namespace APBD_07.Model;

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