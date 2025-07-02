using Microsoft.AspNetCore.Mvc;
using POS_System.Models;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class PermissionController : Controller
    {
        private readonly IPermissionRepository _permision;
        public PermissionController(IPermissionRepository permision)
        {
            _permision = permision;
        }
        public async Task<IActionResult> Index()
        {
            var permissions = await _permision.GetAllPermissionsAsync();
            return View("Index", permissions);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View("Create");
        }
        [HttpPost]
        public async Task<IActionResult> Store(Permission permission)
        {
            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Can not create permission successfully!";
                return View(permission);
            }
            var result = await _permision.AddPermissionAsync(permission);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "Permission has been create successfully!";
                return RedirectToAction("Index");
            }

            return View("Create", permission);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var permission = await _permision.GetPermissionByIdAsync(id);

            return View("Edit", permission);
        }
        [HttpPost]
        public async Task<IActionResult> Update(Permission permission)
        {
            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Can not update permission ...!";
                return View(permission);
            }
            var result = await _permision.UpdatePermissionAsync(permission);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The permission has been update successfully!";
                return RedirectToAction("Index");
            }

            return View("Edit", permission);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _permision.GetPermissionByIdAsync(id);

            return View("Edit", permission);
        }
        [HttpPost]
        public async Task<IActionResult> Destoy(Permission permission)
        {
            if (!ModelState.IsValid)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Can not Delete permission ...!";
                return View(permission);
            }
            var result = await _permision.DeletePermissionAsync(permission);
            if (result)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The permission has been delete successfully!";
                return RedirectToAction("Index");
            }

            return View("Delete", permission);
        }
    }
}
