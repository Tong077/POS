using Microsoft.AspNetCore.Mvc;
using POS_System.Models;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICateogyRepository _categoryRepository;
        public CategoryController(ICateogyRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAll();
            return  View("Index", categories);
        }

        [HttpGet]
        public IActionResult Create(int? count = 1)
        {
            var categories = Enumerable.Range(0, count!.Value).Select(_ => new Category()).ToList();

            return View("Create",categories);
        }
       
        [HttpPost]
        public async Task<IActionResult> Store(List<Category> categories)
        {
            if (categories == null || !categories.Any())
            {
                return View("Create", categories);
            }

            var validCategories = categories.Where(x => !string.IsNullOrWhiteSpace(x.CategoryName)).ToList();
            if (!validCategories.Any())
            {
                return View("Create", validCategories);
            }

            var result = await _categoryRepository.addnew(validCategories);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Create successful...!";
                return RedirectToAction("Index");
            }

            return View("Create", validCategories);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int CategoryId)
        {
            var category = await _categoryRepository.GetByt(CategoryId);
            return View("Edit", category);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", category);
            }
            if (await _categoryRepository.update(category))
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Record Has Been Update successful...!";
                return RedirectToAction("Index");
            }
            return View("Edit", category);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int CategoryId)
        {
            var category = await _categoryRepository.GetByt(CategoryId);
            return View("Delete", category);
        }
        [HttpPost]
        public async Task<IActionResult> Destroy(Category category)
        {
            if (await _categoryRepository.delete(category))
            {
                TempData["toastr-type"] = "warning";
                TempData["toastr-message"] = "The Record Has Been Delete successful...!";
                return RedirectToAction("Index");
            }
            return View("Delete", category);
        }
    }
}
