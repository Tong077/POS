using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models;
using POS_System.Service;

namespace POS_System.Controllers
{
    public class RolesController : Controller
    {
        private readonly IApplicationRoleRepository _roles;
        private readonly EntityConntext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public RolesController(IApplicationRoleRepository roles, EntityConntext context, UserManager<ApplicationUser> userManager)
        {
            _roles = roles;
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var role = await _roles.GetAllRolesAsync();
            return View("Index", role);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Allow access if no roles exist
            if (!await _context.ApplicationRoles.AnyAsync())
            {
                return View("Create", new ApplicationRole());
            }

           
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View("Create", new ApplicationRole());
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Store(ApplicationRole role)
        {
            bool noRolesExisted = !await _context.ApplicationRoles.AnyAsync();
            bool isFirstUser = User.Identity!.IsAuthenticated && !User.IsInRole("Admin");

            if (!ModelState.IsValid)
            {
                return View("Create", role);
            }

            var success = await _roles.create(role);
            if (success)
            {
                // If this is the first role and the user is not in any role, assign it to them
                if (noRolesExisted && isFirstUser)
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.RoleName);
                    }
                }
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "The Role Has Been Create Success Fully...!";
                return RedirectToAction("Index");
            }

            TempData["toastr-type"] = "error";
            TempData["toastr-message"] = "Failed to create role. A role with this name may already exist...!";
           
            return View("Create", role);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var role = await _roles.GetRoleByIdAsync(id);
            return View("Edit",role);
        }
        [HttpPost]
        public async Task<IActionResult> Update(ApplicationRole role)
        {

            if (!ModelState.IsValid)
            {
                return View("Create", role);
            }
            var success = await _roles.update(role);
            if (success)
            {
                return RedirectToAction("Index");
            }
            return View("Edit", role);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = await _roles.GetRoleByIdAsync(id);
            return View("Delete", role);
        }
        [HttpPost]
        public async Task<IActionResult> Destroy(ApplicationRole role)
        {

            if (!ModelState.IsValid)
            {
                return View("Delete", role);
            }
            var success = await _roles.delete(role);
            if (success)
            {
                return RedirectToAction("Index");
            }
            return View("Delete", role);
        }

        //[HttpGet]
       
    }
}
