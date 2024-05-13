using System.Data;
using System.Data.SqlClient;
using apbd07.Models.DTOs;

namespace apbd07.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IConfiguration _configuration;
    public WarehouseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesProductExist(int id)
    {
        var query = "SELECT 1 FROM Product WHERE IdProduct = @ID";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesWarehouseExist(int id)
    {
        var query = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @ID";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesOrderExist(int id, int amount, DateTime createdAt)
    {
        var query = "SELECT 1 FROM [Order] WHERE Amount = @AMOUNT AND IdProduct = @ID AND CreatedAt < @CreatedAt";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;    
    }

    public async Task<bool> DoesOrderCompleted(int id, int amount, DateTime createdAt)
    {
        var query = "Select 1 from Product_Warehouse JOIN [Order] O on Product_Warehouse.IdOrder = O.IdOrder where O.Amount = @AMOUNT AND O.IdProduct = @ID AND O.CreatedAt < @CreatedAt";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;    
    }

    public async Task UpdateOrder(int id, int amount, DateTime createdAt)
    {
        var query = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdProduct = @ID AND Amount = @AMOUNT AND CreatedAt = @CreatedAt";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);
        command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task<int> InsertToProductWarehouse(WarehouseDTO warehouseDto)
    {
        var insert = @"INSERT INTO Product_Warehouse VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
					   SELECT @@IDENTITY AS IdProductWarehouse;";
	    
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        var orderId = await GetOrderId( warehouseDto.IdProduct, warehouseDto.Amount, warehouseDto.CreatedAt);
        var productPrice = await GetProductPrice(warehouseDto.IdProduct);
        
        command.Connection = connection;
        command.CommandText = insert;
	    
        command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
        command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
        command.Parameters.AddWithValue("@IdOrder", orderId);
        command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
        command.Parameters.AddWithValue("@Price", warehouseDto.Amount * productPrice);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
	    
        await connection.OpenAsync();
        
        
        var id = await command.ExecuteScalarAsync();

        if (id is null) throw new Exception();
	    
        return Convert.ToInt32(id);
    }

    public async Task<int> GetOrderId(int id, int amount, DateTime createdAt)
    {
        var orderId = 0;
        var query = "SELECT IdOrder FROM [Order] WHERE Amount = @AMOUNT AND IdProduct = @ID AND CreatedAt < @CreatedAt";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        
        var orderIdOrdinal = reader.GetOrdinal("IdOrder");
        while (await reader.ReadAsync())
        {
            orderId = reader.GetInt32(orderIdOrdinal);
        }
        return orderId;    
    }

    public async Task<double> GetProductPrice(int id)
    {
        var price = 0.0;
        var query = "SELECT Product.Price FROM Product WHERE ID = @ID";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        
        var orderIdOrdinal = reader.GetOrdinal("IdOrder");
        while (await reader.ReadAsync())
        {
            price = reader.GetInt32(orderIdOrdinal);
        }
        return price; 
    }

    public async Task ExecuteProcedure(WarehouseDTO warehouseDto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand("EXEC AddProductToWarehouse @IdProduct, @IdWarehouse, @Amount, @CreatedAt", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
        command.Parameters.AddWithValue("@CreatedAt", warehouseDto.CreatedAt);

        await connection.OpenAsync();
        
        await command.ExecuteNonQueryAsync();

    }
}