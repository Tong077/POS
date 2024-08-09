using Microsoft.EntityFrameworkCore;
using POS_System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("MyConnection");
builder.Services.AddDbContext<EntityConntext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<DapperConnection>();


var app = builder.Build();



app.MapGet("/", () => "Hello World!");

app.Run();
