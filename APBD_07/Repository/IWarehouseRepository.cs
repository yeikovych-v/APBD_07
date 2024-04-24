using APBD_07.Model;

namespace APBD_07.Repository;

public interface IWarehouseRepository
{
    Warehouse? FindById(int id);
}