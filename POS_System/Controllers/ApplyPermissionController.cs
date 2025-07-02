using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Service;
using System.Threading.Tasks;

namespace POS_System.Controllers
{
    public class ApplyPermissionController : Controller
    {
        private readonly IApplyPermissionRepository _permissionRepository;
        private readonly EntityConntext _context;

        public ApplyPermissionController(IApplyPermissionRepository permissionRepository, EntityConntext context)
        {
            _permissionRepository = permissionRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return View(permissions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName");
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name");
            return View(new RolePermission());
        }

        [HttpPost]
        public async Task<IActionResult> Store(RolePermission model)
        {


            if (ModelState.IsValid)
            {
                var result = await _permissionRepository.AddPermissionAsync(model.UserID, model.PermissionId);
                if (result)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "Create Permission successfully!";
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to add permission.");
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName");
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name");
            return View("Create", model);
        }

        [HttpGet]

        public async Task<IActionResult> Edit(int id)
        {
            var rolePermission = await _permissionRepository.GetByIdAsync(id);
            if (rolePermission == null)
            {
                return NotFound();
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", rolePermission.UserID);
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name", rolePermission.PermissionId);
            return View(rolePermission);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(int id, RolePermission model)
        {
            if (ModelState.IsValid)
            {
                var result = await _permissionRepository.EditAsync(id, model.UserID, model.PermissionId);
                if (result)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "Update Permission successfully!";
                    return RedirectToAction("Index");
                }
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Failed to update permission ...!";
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", model.UserID);
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name", model.PermissionId);
            return View(model);
        }

        [HttpGet]

        public async Task<IActionResult> Delete(int id)
        {

            var rolePermission = await _permissionRepository.GetByIdAsync(id);
            if (rolePermission == null)
            {
                return NotFound();
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", rolePermission.UserID);
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name", rolePermission.PermissionId);
            return View(rolePermission);
        }

        [HttpPost]

        public async Task<IActionResult> Delete(int id, RolePermission model)
        {
            if (ModelState.IsValid)
            {
                var result = await _permissionRepository.DeleteAsync(id, model.UserID, model.PermissionId);
                if (result)
                {
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "Update Permission successfully!";
                    return RedirectToAction("Index");
                }
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Failed to update permission ...!";
            }

            ViewBag.Users = new SelectList(_context.Users, "Id", "UserName", model.UserID);
            ViewBag.Permissions = new SelectList(_context.Permissions, "PermissionId", "Name", model.PermissionId);
            return View(model);
        }
    }
}