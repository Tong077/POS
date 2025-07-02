using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;

using POS_System.Data;
using POS_System.Middleware;
using POS_System.Models;
using POS_System.Service;
using POS_System.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("MyConnection");
builder.Services.AddDbContext<EntityConntext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddScoped<DapperConnection>();
builder.Services.AddScoped<ICateogyRepository, CategoryService>();
builder.Services.AddScoped<ICustomerRepository, CustomerService>();
builder.Services.AddScoped<IProductRepository, ProductService>();
builder.Services.AddScoped<ISupplierRepository, SupplierService>();
builder.Services.AddScoped<IInventoryRepository, InventoryService>();
builder.Services.AddScoped<IPosRepository, PosService>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyService>();
builder.Services.AddScoped<IApplicationRoleRepository, ApplicationRoleService>();
builder.Services.AddScoped<IApplyPermissionRepository, ApplyPermissionService>();
builder.Services.AddScoped<IPermissionRepository, PermissionService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicAuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IPolicyRepositovy, PolicyService>();


builder.Services.AddHttpContextAccessor();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = false;
    options.User.AllowedUserNameCharacters = ""; // Remove restrictions on username characters
})
.AddEntityFrameworkStores<EntityConntext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
    });


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorizationLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();








