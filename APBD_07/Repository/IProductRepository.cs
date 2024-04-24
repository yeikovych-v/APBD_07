using APBD_07.Model;

namespace APBD_07.Repository;

public interface IProductRepository
{

    public Product? FindById(int id);
}