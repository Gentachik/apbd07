using System.Data.SqlClient;
using apbd07.Models;
using apbd07.Models.DTO_s;

namespace apbd07.Services;

public class WarehouseService
{
    private SqlConnection connection;

    public WarehouseService(SqlConnection connection)
    {
        this.connection = connection;
    }

    public bool CheckProduct(ProductWarehouse request)
    {
        if (request.Amount <=0)
        {
            return false;
        }
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        var product = GetProduct(request);
        if (product == null)
        {
            return false;
        }
        command.CommandText = "SELECT count(*) FROM Warehouse w WHERE w.IdWarehouse == @IdWarehouse";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        int warehouse = (int) command.ExecuteScalar();
        return warehouse != 0;
    }

    public bool CheckOrder(ProductWarehouse request)
    {
        var order = GetOrder(request);
        return order != null;
    }

    public bool CheckProductWarehouse(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT count(*) FROM Product_Warehouse pwa WHERE pwa.IdOrder == @IdOrder";
        var order = GetOrder(request);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        int productWarehouse = (int) command.ExecuteScalar();
        if (productWarehouse == 0)
        {
            return false;
        }
        return true;
    }

    public void UpdateFulfilledAt(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        var order = GetOrder(request);
        command.Connection = connection;
        command.CommandText = "UPDATE Order SET FulfilledAt=@FulfilledAt WHERE IdOrder=@IdOrder";
        command.Parameters.AddWithValue("FulfilledAt", DateTime.Now);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.ExecuteNonQuery();
    }

    public void InsertRequest(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        var order = GetOrder(request);
        var product = GetProduct(request);
        command.CommandText = "INSERT INTO Product_Warehouse Values (@IdWarehouse,@IdProduct,@IdOrder,@Amount,@Price,@CreatedAt)";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("Price", request.Amount * product.Price);
        command.Parameters.AddWithValue("CreatedAt", DateTime.Now);
        command.ExecuteNonQuery();
    }

    public int GetIdOfRequest(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        var product = GetProduct(request);
        var order = GetOrder(request);
        command.CommandText =
            "SELECT IdProductWarehouse FROM Product_Warehouse WHERE IdWarehouse==@IdWarehouse AND IdProduct==@IdProduct AND IdOrder==@IdOrder AND Amount==@Amount AND Price==@Price AND CreatedAt==@CreatedAt";
        command.Parameters.AddWithValue("IdWarehouse", request.IdWarehouse);
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("IdOrder", order.IdOrder);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("Price", request.Amount * product.Price);
        command.Parameters.AddWithValue("CreatedAt", DateTime.Now);
        var idProductWarehouse = (int) command.ExecuteScalar();
        return idProductWarehouse;
    }
    private Order? GetOrder(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT * FROM Order o WHERE o.IdProduct == @IdProduct AND o.Amount == @Amount AND o.CreatedAt < @RequestCreatedAt";
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        command.Parameters.AddWithValue("Amount", request.Amount);
        command.Parameters.AddWithValue("RequestCreatedAt", request.CreatedAt);
        var order = (Order) command.ExecuteScalar();
        return order;
    }

    private Product? GetProduct(ProductWarehouse request)
    {
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT * FROM Product p WHERE p.IdProduct ==@IdProduct";
        command.Parameters.AddWithValue("IdProduct", request.IdProduct);
        var product = (Product) command.ExecuteScalar();
        return product;
    }
}