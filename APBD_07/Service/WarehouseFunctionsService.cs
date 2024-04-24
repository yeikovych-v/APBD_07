using APBD_07.Model;
using APBD_07.Repository;

namespace APBD_07.Service;

public class WarehouseFunctionsService(IProductRepository productRepository, IOrderRepository orderRepository, IWarehouseRepository warehouseRepository, IWarehouseProductRepository warehouseProductRepository)
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
        return warehouseProductRepository.InsertCompletedOrder(warehouseId, order.Product.Id, order.Id, order.Amount, order.Amount * order.Product.Price, DateTime.Now);
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