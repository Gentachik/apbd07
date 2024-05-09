using System.Data.SqlClient;
using apbd07.Models;
using apbd07.Models.DTO_s;
using Microsoft.AspNetCore.Mvc;

namespace apbd07.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public WarehouseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get(ProductWarehouse request)
    {
        //1
        if (request.Amount <=0)
        {
            return BadRequest("There amount value smaller or equal to 0");
        }
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT * FROM Product p WHERE p.IdProduct ==@IdProduct";
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        var product = (Product) command.ExecuteScalar();
        if (product == null)
        {
            return BadRequest("There is no product with such id");
        }
        command.CommandText = "SELECT count(*) FROM Warehouse w WHERE w.IdWarehouse == @IdWarehouse";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        int warehouse = (int) command.ExecuteScalar();
        if (warehouse == 0)
        {
            return BadRequest("There is no warehouse with such id");
        }
        //2
        command.CommandText = "SELECT * FROM Order o WHERE o.IdProduct == @IdProduct AND o.Amount == @Amount AND o.CreatedAt < @RequestCreatedAt";
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("RequestCreatedAt", request.CreatedAt);
        var order = (Order) command.ExecuteScalar();
        if (order==null)
        {
            return BadRequest("There is no order with such id or amount or createdAt time");
        }
        //3
        command.CommandText = "SELECT count(*) FROM Product_Warehouse pwa WHERE pwa.IdOrder == @IdOrder";
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        int productWarehouse = (int) command.ExecuteScalar();
        if (productWarehouse == 0)
        {
            return BadRequest("There is no warehouse with such id");
        }
        //4
        command.CommandText = "UPDATE Order SET FulfilledAt=@FulfilledAt WHERE IdOrder=@IdOrder";
        command.Parameters.AddWithValue("FulfilledAt", DateTime.Now);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.ExecuteNonQuery();
        //5
        command.CommandText = "INSERT INTO Product_Warehouse Values (@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,@CreatedAt)";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("Price", request.Amount * product.Price);
        command.Parameters.AddWithValue("CreatedAt", DateTime.Now);
        command.ExecuteNonQuery();
        //6
        command.CommandText =
            "SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdWarehouse==@IdWarehouse AND IdProduct==@IdProduct AND IdOrder==@IdOrder AND Amount==@Amount AND Price==@Price AND CreatedAt==@CreatedAt";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("Price", request.Amount * product.Price);
        command.Parameters.AddWithValue("CreatedAt", DateTime.Now);
        int idProductWarehouse = (int) command.ExecuteScalar();
        return Ok(idProductWarehouse);
    }
}