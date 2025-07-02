using Microsoft.AspNetCore.Mvc;
using POS_System.Models;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class SupllierController : Controller
    {
        private readonly ISupplierRepository _supplierRepository;
        public SupllierController(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _supplierRepository.GetAll();
            return View("Index", result);
        }

        public ActionResult Create(int? count = 1)
        {
            
            var suppliers = Enumerable.Range(0, count!.Value).Select(_ => new Supplier()).ToList();
            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(List<Supplier> suppliers)
        {
            if (suppliers == null || !suppliers.Any())
            {
                ModelState.AddModelError("", "Please enter at least one valid supplier.");

                return View("Create", suppliers);
            }

           
            var validSuppliers = suppliers
                .Where(s => !string.IsNullOrWhiteSpace(s.SupplierName))
                .ToList();

            if (!validSuppliers.Any())
            {
                ModelState.AddModelError("", "Please enter at least one valid supplier.");
                return View("Create", suppliers);
            }
            var result = await _supplierRepository.Addrang(validSuppliers);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "Supplier Has Been Create Success Fully...!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to create suppliers. Please try again.");
            return View("Create", suppliers);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int SupplierId)
        {
            var sup = await _supplierRepository.GetById(SupplierId);
            return View("Edit", sup);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", supplier);
            }
            var result = await _supplierRepository.Update(supplier);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "Sale Has Been Update Success Fully...!";
                return RedirectToAction("Index", result);
            }
            return View("Edit", supplier);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int SupplierId)
        {
            var result = await _supplierRepository.GetById(SupplierId);
            return View("Delete", result);
        }
        [HttpPost]
        public async Task<IActionResult> Destroy(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View("Delete", supplier);
            }
            var result = await _supplierRepository.Delete(supplier);
            if (result)
            {
                return RedirectToAction("Index", result);
            }
            return View("Delete", supplier);
        }


    }
}
