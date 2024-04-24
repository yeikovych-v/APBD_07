using APBD_07.Dto;
using APBD_07.Model;
using APBD_07.Service;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace APBD_07.Controller;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController(WarehouseFunctionsService funcService, ProcedureService prodService)
{
    [HttpPost]
    [Route("/transaction")]
    public IResult GetProductFromWarehouseTransaction([FromBody] OrderJsonDto orderDto)
    {
        var productExists = funcService.ProductExists(orderDto.IdProduct);
        if (!productExists) return BadRequest($"Product with Id {orderDto.IdProduct} does not exist");

        var warehouseExists = funcService.WarehouseExists(orderDto.IdWarehouse);
        if (!warehouseExists) return BadRequest($"Warehouse with Id {orderDto.IdWarehouse} does not exist");

        var orderExists = funcService.OrderExists(orderDto.IdProduct, orderDto.Amount, orderDto.Created);
        if (!orderExists)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var order = funcService.GetOrder(orderDto.IdProduct, orderDto.Amount);

        if (order == null)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var wasOrderCompleted = funcService.WasOrderCompleted(order);
        if (wasOrderCompleted)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] was already completed");

        funcService.SetFulfilledTime(order);

        if (!funcService.AddCompletedOrder(order, orderDto.IdWarehouse))
            return BadRequest($"Order: [{order}] was not completed.");

        var completedOrderId = funcService.GetCompletedOrderId(order.Id, orderDto.IdProduct, orderDto.IdWarehouse);

        return Ok("Got id: " + completedOrderId);
    }

    [HttpPost]
    [Route("/procedure")]
    public IResult GetProductFromWarehouseProcedure([FromBody] OrderJsonDto orderDto)
    {
        var productExists = funcService.ProductExists(orderDto.IdProduct);
        if (!productExists) return BadRequest($"Product with Id {orderDto.IdProduct} does not exist");

        var warehouseExists = funcService.WarehouseExists(orderDto.IdWarehouse);
        if (!warehouseExists) return BadRequest($"Warehouse with Id {orderDto.IdWarehouse} does not exist");

        var orderExists = funcService.OrderExists(orderDto.IdProduct, orderDto.Amount, orderDto.Created);
        if (!orderExists)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var order = funcService.GetOrder(orderDto.IdProduct, orderDto.Amount);

        if (order == null)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] does not exist");

        var wasOrderCompleted = funcService.WasOrderCompleted(order);
        if (wasOrderCompleted)
            return BadRequest(
                $"Order with product Id [{orderDto.IdProduct}] and Amount [{orderDto.Amount}] was already completed");
        try
        {
            prodService.ExecuteSetAddProcedure(orderDto.IdWarehouse, orderDto.IdProduct, orderDto.Amount);
        }
        catch (Exception e)
        {
            return BadRequest("Error occured while trying to execute procedure.");
        }

        var completedOrderId = funcService.GetCompletedOrderId(order.Id, orderDto.IdProduct, orderDto.IdWarehouse);

        return Ok("Got id: " + completedOrderId);
    }
}