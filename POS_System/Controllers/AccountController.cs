using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using POS_System.Data;
using POS_System.Models;
using POS_System.Models.DTO;
using POS_System.Service;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POS_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EntityConntext _context;
        private readonly IHttpContextAccessor _accessor;
      
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            EntityConntext context,
            IHttpContextAccessor accessor
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            this._accessor = accessor;
           
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    DisplayUsername = user.DisplayUsername ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList(),
                    ImagePath = user.ImagePath ?? string.Empty,

                });
            }

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var model = new RegisterDTO();


            ViewBag.Roles = new SelectList(
                _context.ApplicationRoles.Where(r => r.IsActive).ToList(),
                "RoleName",
                "RoleName");
            return View("Register", model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    DisplayUsername = model.Username
                };

                // Handle image upload
                if (model.Image != null && model.Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Image", "Only JPG, JPEG, PNG, or GIF files are allowed.");
                    }
                    else if (model.Image.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Image", "Image file size must be less than 5MB.");
                    }
                    else
                    {
                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(stream);
                        }
                        user.ImagePath = $"/images/users/{fileName}";
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(
                        await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                        "RoleName",
                        "RoleName"
                    );
                    return View("Register", model);
                }

                // Create the user
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        var selectedRole = await _context.ApplicationRoles
                            .Where(r => r.IsActive && r.RoleName.ToLower() == model.SelectedRole.ToLower())
                            .FirstOrDefaultAsync();

                        if (selectedRole != null)
                        {
                            await _userManager.AddToRoleAsync(user, selectedRole.RoleName);
                        }
                        else
                        {
                            ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' is not valid or active.");

                            TempData["toastr-type"] = "error";
                            TempData["toastr-message"] = "Is not valid or Active...!";
                        }
                    }
                    else
                    {

                        TempData["toastr-type"] = "error";
                        TempData["toastr-message"] = "A role must be selected.";
                    }

                    if (!ModelState.IsValid)
                    {
                        ViewBag.Roles = new SelectList(
                            await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                            "RoleName",
                            "RoleName"
                        );
                        return View("Register", model);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "The User Has Been Create Success Fully...!";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = new SelectList(
                await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                "RoleName",
                "RoleName"
            );
            return View("Register", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string ID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(ID);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRole = userRoles.FirstOrDefault();

            var model = new EditUserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                DisplayUsername = user.DisplayUsername,
                Email = user.Email,
                SelectedRole = selectedRole,
                CurrentImagePath = user.ImagePath
            };
            ViewBag.Roles = new SelectList(roles, "RoleName", "RoleName", selectedRole);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditUserDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }


                user.UserName = model.DisplayUsername;
                user.DisplayUsername = model.DisplayUsername;
                user.Email = model.Email;

               
                if (model.Image != null && model.Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Image", "Only JPG, JPEG, PNG, or GIF files are allowed.");
                    }
                    else if (model.Image.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Image", "Image file size must be less than 5MB.");
                    }
                    else
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(user.ImagePath))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(stream);
                        }

                        user.ImagePath = $"/images/users/{fileName}";
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(
                        await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                        "RoleName",
                        "RoleName",
                        model.SelectedRole
                    );
                    return View(model);
                }


                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    var currentUserId = _userManager.GetUserId(User);
                    if (user.Id == currentUserId)
                    {
                        _accessor.HttpContext.Session.SetString("UserName", user.DisplayUsername ?? user.UserName); // Update session with DisplayUsername
                        _accessor.HttpContext.Session.SetString("Image", string.IsNullOrEmpty(user.ImagePath) ? "wwwroot/images/users" : user.ImagePath);
                    }

                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        var roles = await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync();
                        var selectedRole = roles.FirstOrDefault(r => r.RoleName.ToLower() == model.SelectedRole.ToLower());
                        if (selectedRole != null)
                        {
                            await _userManager.AddToRoleAsync(user, selectedRole.RoleName);
                        }
                        else
                        {
                            ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' is not valid or active.");
                            ViewBag.Roles = new SelectList(roles, "RoleName", "RoleName", model.SelectedRole);
                            return View(model);
                        }
                    }
                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "The User Has Been Update Success Fully...!";
                    return RedirectToAction("Index");
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

         
            ViewBag.Roles = new SelectList(
                await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                "RoleName",
                "RoleName",
                model.SelectedRole
            );
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Profile(string id)

        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRole = userRoles.FirstOrDefault();

            var model = new EditUserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                DisplayUsername = user.DisplayUsername,
                Email = user.Email,
                SelectedRole = selectedRole,
                CurrentImagePath = user.ImagePath
            };
            ViewBag.Roles = new SelectList(roles, "RoleName", "RoleName", selectedRole);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProFile(EditUserDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.DisplayUsername;
                user.DisplayUsername = model.DisplayUsername;
                user.Email = model.Email;

                if (model.Image != null && model.Image.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Image", "Only JPG, JPEG, PNG, or GIF files are allowed.");
                    }
                    else if (model.Image.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Image", "Image file size must be less than 5MB.");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(user.ImagePath))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/users", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(stream);
                        }

                        user.ImagePath = $"/images/users/{fileName}";
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(
                        await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                        "RoleName",
                        "RoleName",
                        model.SelectedRole
                    );
                    return View(model);
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    var currentUserId = _userManager.GetUserId(User);
                    if (user.Id == currentUserId)
                    {
                        _accessor.HttpContext.Session.SetString("UserName", user.DisplayUsername ?? user.UserName);
                        _accessor.HttpContext.Session.SetString("Image", string.IsNullOrEmpty(user.ImagePath) ? "wwwroot/images/users" : user.ImagePath);
                    }

                    if (User.IsInRole("Admin"))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        }

                        if (!string.IsNullOrEmpty(model.SelectedRole))
                        {
                            var roles = await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync();
                            var selectedRole = roles.FirstOrDefault(r => r.RoleName.ToLower() == model.SelectedRole.ToLower());
                            if (selectedRole != null)
                            {
                                await _userManager.AddToRoleAsync(user, selectedRole.RoleName);
                            }
                            else
                            {
                                ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' is not valid or active.");
                                ViewBag.Roles = new SelectList(roles, "RoleName", "RoleName", model.SelectedRole);
                                return View(model);
                            }
                        }
                    }

                    TempData["toastr-type"] = "success";
                    TempData["toastr-message"] = "The User Has Been Updated Successfully...!";
                    return RedirectToAction("Profile", new { id = model.Id });
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            ViewBag.Roles = new SelectList(
                await _context.ApplicationRoles.Where(r => r.IsActive).ToListAsync(),
                "RoleName",
                "RoleName",
                model.SelectedRole
            );
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(Id);
            if ((user == null))
            {
                return NotFound();
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = userRoles.ToList(),
                ImagePath = user.ImagePath
            };
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> Delete(UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

           
            if (!string.IsNullOrEmpty(user.ImagePath))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // If deletion fails, re-populate the model and return the view
            var userRoles = await _userManager.GetRolesAsync(user);
            model.Username = user.UserName;
            model.Email = user.Email;
            model.Roles = userRoles.ToList();
            model.ImagePath = user.ImagePath;

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginDTO
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }


        
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Invalid email or password.";
                return View(model);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Invalid email or password.";
                return View(model);
            }

            var permissionService = _accessor.HttpContext.RequestServices.GetRequiredService<IApplyPermissionRepository>();
            var permissions = await permissionService.GetUserPermissionsAsync(user.Id);
           

            var claims = new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName)
                };
            claims.AddRange(permissions.Select(p => new Claim("Permission", p)));
           
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = false });
            

            _accessor.HttpContext.Session.SetString("UserName", user.UserName ?? user.UserName);
            _accessor.HttpContext.Session.SetString("Image", string.IsNullOrEmpty(user.ImagePath) ? "wwwroot/images/users" : user.ImagePath);

            TempData["toastr-type"] = "success";
            TempData["toastr-message"] = "Login successful...!";
            return RedirectToLocal(model.ReturnUrl);
        }
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            TempData["toastr-type"] = "success";
            TempData["toastr-message"] = "Login successful...!";
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _signInManager.SignOutAsync();


            TempData.Clear();
            TempData["toastr-type"] = "success";
            TempData["toastr-message"] = "Sign out successful...!";
            return RedirectToAction("Login", "Account");
        }


       
    }
}

    

