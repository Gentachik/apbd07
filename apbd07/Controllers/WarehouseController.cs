using System.Data.SqlClient;
using apbd07.Models;
using apbd07.Models.DTO_s;
using apbd07.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private WarehouseService warehouseService;

    public WarehouseController(IConfiguration configuration)
    {
        _configuration = configuration;
        warehouseService = new WarehouseService(new SqlConnection(_configuration.GetConnectionString("Default")));
    }

    [HttpPost]
    public IActionResult UpdateRequestId(ProductWarehouse request)
    {
        //1
        if (!warehouseService.CheckProduct(request))
        {
            return BadRequest("There problem with product");
        }
        //2
        if (!warehouseService.CheckOrder(request))
        {
            return BadRequest("There problem with order");
        }
        //3
        if (!warehouseService.CheckProductWarehouse(request))
        {
            return BadRequest("There problem with warehouse");
        }
        //4
        warehouseService.UpdateFulfilledAt(request);
        //5
        warehouseService.InsertRequest(request);
        //6
        var id = warehouseService.GetIdOfRequest(request);
        return Ok(id);
    }
}