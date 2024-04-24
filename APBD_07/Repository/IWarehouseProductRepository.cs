using APBD_07.Model;

namespace APBD_07.Repository;

public interface IWarehouseProductRepository
{
    bool InsertCompletedOrder(int warehouseId, int productId, int orderId, int amount, decimal orderPrice,
        DateTime createdAt);

    WarehouseProduct? FindByOrderAndProductAndWarehouseId(int orderId, int productId, int warehouseId);
}