using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly EntityConntext context;
        private readonly IPosRepository _pos;
        private readonly ICustomerRepository _customer;
        public HomeController(EntityConntext context, IPosRepository pos, ICustomerRepository customer  )
        {
            this._pos = pos;
            this.context = context;
            _customer = customer;
        }
        public async Task <IActionResult> Index()
        {

            int months = DateTime.Now.Month;
            int years = DateTime.Now.Year;
          

            int totalProductsSold = await _pos.GetTotalProductsSold(months, years);
            ViewBag.TotalProductsSold = totalProductsSold > 0 ? totalProductsSold : 0;

            DateTime targetDate =DateTime.UtcNow; 
            string monthYear = targetDate.ToString("dd - MMMM - yyyy");
            ViewBag.MonthYear = monthYear;




            decimal profitKHR = await _pos.GetProfit(months, years, "KHR");
            decimal profitUSD = await _pos.GetProfit(months, years, "USD");

            ViewBag.ProfitKHR = profitKHR;
            ViewBag.ProfitUSD = profitUSD;


           var customer = await _customer.GetAll();
            int cuscount = customer.Count();
            ViewData["CustomerCount"] = cuscount;

            return View("Index");
        }


        [HttpGet]
        public async Task<IActionResult> GetChartData(int? month, int? year)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;

            var salesData = await _pos.GetProductSalesData(month.Value, year.Value);

            var chartData = new
            {
                labels = salesData.Select(x => x.ProductName).ToArray(),
                datasets = new[]
    {
        new
        {
            label = "Products Sold",
            data = salesData.Select(x => x.Quantity).ToArray(),
            backgroundColor = salesData.Select((_, index) =>
                $"rgba({(index * 50 % 255)}, {(index * 100 % 255)}, {(index * 150 % 255)}, 0.2)").ToArray(),
            borderColor = salesData.Select((_, index) =>
                $"rgba({(index * 50 % 255)}, {(index * 100 % 255)}, {(index * 150 % 255)}, 1)").ToArray(),
            borderWidth = 1
        }
    }
            };

            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalProductsSold(int month, int year)
        {
            int totalProductsSold = await _pos.GetTotalProductsSold(month, year);
            return Json(new { total = totalProductsSold });
        }

    }
}
