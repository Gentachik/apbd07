using apbd07.Models.DTOs;
using apbd07.Repositories;
using apbd07.Service;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private IWarehouseRepository _warehouseRepository;
    private WarehouseService _warehouseService;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
        _warehouseService = new WarehouseService();
    }

    [HttpPost]
    [Route("task1")]
    public async Task<IActionResult> Task1(WarehouseDTO warehouseDto)
    {
        //1
        if (!await _warehouseRepository.DoesProductExist(warehouseDto.IdProduct))
            return NotFound($"Product with given ID - {warehouseDto.IdProduct} doesn't exist");
        if (!await _warehouseRepository.DoesWarehouseExist(warehouseDto.IdWarehouse))
            return NotFound($"Warehouse with given ID - {warehouseDto.IdWarehouse} doesn't exist");
        if (!_warehouseService.DoesAmountPositive(warehouseDto.Amount))
            return BadRequest("Amount should be a positive value");
        //2
        if (!await _warehouseRepository.DoesOrderExist(warehouseDto.IdProduct, warehouseDto.Amount, warehouseDto.CreatedAt))
            return NotFound($"Order with provided ID product - {warehouseDto.IdProduct} and amount - {warehouseDto.Amount} and after this date - {warehouseDto.CreatedAt} doesn't exist");
        //3
        if (!await _warehouseRepository.DoesOrderCompleted(warehouseDto.IdProduct, warehouseDto.Amount, warehouseDto.CreatedAt))
            return NotFound($"Order with provided ID product - {warehouseDto.IdProduct} and amount - {warehouseDto.Amount} and after this date - {warehouseDto.CreatedAt} has been already completed");
        //4
        await _warehouseRepository.UpdateOrder(warehouseDto.IdProduct, warehouseDto.Amount, warehouseDto.CreatedAt);
        //5
        var id = _warehouseRepository.InsertToProductWarehouse(warehouseDto);
        //6
        return Ok(id);
    }

    [HttpPost]
    [Route("task2")]
    public async Task<IActionResult> Task2(WarehouseDTO warehouseDto)
    {
        var id = await _warehouseRepository.ExecuteProcedure(warehouseDto);
        if (id == 0)
            return BadRequest();
        return Ok(id);
    }

}