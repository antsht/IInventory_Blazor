using Microsoft.EntityFrameworkCore;
using InventoryApp.Components;
using InventoryApp.Data;
using InventoryApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SQLite database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=inventory.db";

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddScoped<BarcodeService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<WorkplaceService>();

var app = builder.Build();

// Ensure database is created and migrations applied
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = await factory.CreateDbContextAsync();
    await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
