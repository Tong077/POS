using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Service;
using static System.Net.Mime.MediaTypeNames;

namespace POS_System.Controllers
{
    public class ProductController : Controller
    {
        private readonly EntityConntext _conntext;
        private readonly IProductRepository _product;
        private readonly ICateogyRepository _cateogy;
        private readonly ISupplierRepository _supplier;
        private readonly IWebHostEnvironment _enviroment;
        public ProductController(IProductRepository product, ICateogyRepository cateogy, ISupplierRepository supplier, IWebHostEnvironment enviroment, EntityConntext context)
        {
            _conntext = context;
            _product = product;
            _cateogy = cateogy;
            _supplier = supplier;
            _enviroment = enviroment;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _product.GetAll();

            return View("Index", result);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var category = await _conntext.categories.ToListAsync();

            ViewBag.Category = new SelectList(category, "CategoryId", "CategoryName");

            //var supplier = await _supplier.GetAll();
            var supplier = await _conntext.suppliers.ToListAsync();
            ViewBag.Supplier = new SelectList(supplier, "SupplierId", "SupplierName");

            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Store(Product product, IFormFile? file)
        {

            if (file != null && file.Length > 0)
            {
                string filename = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var directoryPath = Path.Combine(_enviroment.WebRootPath, "images");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, filename);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                product.Image = filename;
            }
            else
            {

                product.Image = null;
            }

            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Faild To add The Product ...!";
                return View("Create", product);
            }
            var result = await _product.Create(product);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Product Has Been add Success Fully...!";
                return RedirectToAction("Index");
            }
            return View("Create", product);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int productId)
        {
            
            var categories = await _conntext.categories.ToListAsync();
           

            var suppliers = await _conntext.suppliers.ToListAsync();
          

          
            var product = await _product.GetById(productId);
            if (product == null)
            {
                return NotFound(); 
            }

           
            ViewBag.Category = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.Supplier = new SelectList(suppliers, "SupplierId", "SupplierName", product.SupplierId);

            return View("Edit", product);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int productId, Product product, IFormFile? file)
        {
            var existpro = await _product.GetById(productId);
            if (existpro == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0)
            {
                string filename = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var directory = Path.Combine(_enviroment.WebRootPath, "images");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, filename);

                // Delete old image 
                if (!string.IsNullOrEmpty(existpro.Image))
                {
                    string oldFilePath = Path.Combine(directory, existpro.Image);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                existpro.Image = filename;
            }
            // keep the  image if no new image is uploaded
            else
            {
                existpro.Image = existpro.Image;
            }

           
            existpro.ProductName = product.ProductName;
            existpro.Price = product.Price;
            existpro.Currency = product.Currency;
            existpro.CategoryId = product.CategoryId;
            existpro.SupplierId = product.SupplierId;
            existpro.StockQuantity = product.StockQuantity;
            existpro.Description = product.Description;

            if (!ModelState.IsValid)
            {
                return View("Edit", existpro);
            }
            var result = await _product.update(existpro);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Product Has Been updated Success Fully...!";
                return RedirectToAction("Index",result);
            }

            return View("Edit", existpro);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int  productId)
        {
            var categories = await _conntext.categories.ToListAsync();

            var suppliers = await _conntext.suppliers.ToListAsync();
            


            var product = await _product.GetById(productId);
            if (product == null)
            {
                return NotFound();
            }


            ViewBag.Category = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewBag.Supplier = new SelectList(suppliers, "SupplierId", "SupplierName", product.SupplierId);

            return View("Delete", product);
        }
        [HttpPost]
        public async Task<IActionResult> Destroy(Product product)
        {
            if (!string.IsNullOrEmpty(product.Image))
            {
                var directory = Path.Combine(_enviroment.WebRootPath, "images");
                string filepath = Path.Combine(directory, product.Image);
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
            }
            var result = await _product.delete(product);
            if (result)
            {
                return RedirectToAction("Index");
            }
            return View("Delete");
        }
    }
}
