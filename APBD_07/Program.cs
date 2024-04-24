using APBD_07.Repository;
using APBD_07.Service;

namespace APBD_07;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers().AddXmlSerializerFormatters();
        builder.Services.AddScoped<IOrderRepository, MsSqlOrderDb>();
        builder.Services.AddScoped<IProductRepository, MsSqlProductDb>();
        builder.Services.AddScoped<IWarehouseRepository, MsSqlWarehouseDb>();
        builder.Services.AddScoped<IWarehouseProductRepository, MsqlWarehouseProductRepository>();
        builder.Services.AddScoped<WarehouseFunctionsService>();
        builder.Services.AddScoped<ProcedureService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
    
}