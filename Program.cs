using Microsoft.EntityFrameworkCore;
using ParqueoMvc.Data;
using ParqueoMvc.Repositories.Database;
using ParqueoMvc.Repositories.Interfaces;
using ParqueoMvc.Repositories.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var dataSource = builder.Configuration["DataSource:Provider"] ?? "Database";
switch (dataSource.Trim())
{
    case "Database":
        var connectionString = builder.Configuration.GetConnectionString("MySecretPostgresConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'MySecretPostgresConnection' is not configured.");

        builder.Services.AddDbContext<ParqueoDbContext>(options =>
            options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IAutomovilRepository, AutomovilRepository>();
        builder.Services.AddScoped<IParqueoRepository, ParqueoRepository>();
        builder.Services.AddScoped<IIngresoAutomovilRepository, IngresoAutomovilRepository>();
        break;
    case "Json":
        builder.Services.AddSingleton<JsonDataSource>();
        builder.Services.AddScoped<IAutomovilRepository, AutomovilJsonRepository>();
        builder.Services.AddScoped<IParqueoRepository, ParqueoJsonRepository>();
        builder.Services.AddScoped<IIngresoAutomovilRepository, IngresoAutomovilJsonRepository>();
        break;
    default:
        throw new InvalidOperationException(
            $"El proveedor de datos '{dataSource}' no es válido. Use 'Database' o 'Json'.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
