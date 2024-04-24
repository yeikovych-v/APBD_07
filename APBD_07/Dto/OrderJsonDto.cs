namespace APBD_07.Dto;

public class OrderJsonDto(int idProduct, int idWarehouse, int amount, DateTime created)
{
    public int IdProduct { get; set; } = idProduct;
    public int IdWarehouse { get; set; } = idWarehouse;
    public int Amount { get; set; } = amount;
    public DateTime Created { get; set; } = created;
}