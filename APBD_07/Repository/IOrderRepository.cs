using APBD_07.Model;

namespace APBD_07.Repository;

public interface IOrderRepository
{
    Order? FindByProductAndAmount(int productId, int amount);
    bool UpdateById(int id, Order order);

    public Order? FindByOrderAndProductId(int orderId, int productId);
}