using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Service;
using X.PagedList.Extensions;

namespace POS_System.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customer;
        private readonly EntityConntext _context;
        public CustomerController(ICustomerRepository customer, EntityConntext cont)
        {
            _customer = customer;
            _context = cont;
        }
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10; // Number of records per page
            int pageNumber = page ?? 1; // Default to the first page if no page number is provided

            var cut = await _customer.GetAll();
            var pageList = cut.ToPagedList(pageNumber, pageSize);
            return View("Index",pageList);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }
        [HttpPost]
        public async Task<IActionResult> Stores(Customer custmer)
        {
            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "wearning";
                TempData["toastr-message"] = "Fail To Created...!";
                return View("Create", custmer);
            }
            var cus = await _customer.addnew(custmer);
            if (cus)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Create successful...!";
                return RedirectToAction("Index", cus);
            }
            return View("Create");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int CustomerId)
        {
            var cus = await _customer.GetById(CustomerId);
            return View("Edit", cus);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "wearning";
                TempData["toastr-message"] = "Fail To Update...!";
                return View("Edit", customer);
            }
            var cus = await _customer.update(customer);
            if (cus)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Update successful...!";
                return RedirectToAction("Index", cus);
            }
            return View("Edit", customer);
        }
        public async Task<IActionResult> Delete(int CustomerId)
        {
            var cus = await _customer.GetById(CustomerId);
            return View("Delete", cus);
        }
        [HttpPost]
        public async Task<IActionResult> Destroy(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View("Delete", customer);
            }
            var cus = await _customer.delete(customer);
            if (cus)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Delete successful...!";
                return RedirectToAction("Index", cus);
            }
            return View("Delete", customer);
        }


      
    }
}

   