using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using net08.Models;
using net08.Data;
using net08.Models.ViewModels;

namespace net08.Controllers;

[AllowAnonymous]
public class AccountController : Controller {

    private readonly ILogger logger;
    private readonly IWebHostEnvironment env;
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly ApplicationDbContext context;

    public AccountController(ILogger<AccountController> logger, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment env, ApplicationDbContext context) {
        this.logger = logger;
        this.env = env;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.context = context;
    }
    
    // GET pro /Account/Login
    // Zobrazí úvodní obrazovku pro přihlášení
    public IActionResult Login() {
        return View();
    }

    // POST pro /Account/Login
    // Pokud účet existuje, přihlásí uživatele a vrátí jej na úvodní stránku.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (ModelState.IsValid) {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) {
                ModelState.AddModelError(string.Empty, "Účet neexistuje.");
                return View(model);
            }
        
            var result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded) {
                return RedirectToAction("Feed", "Home");
            }
        }
        ModelState.AddModelError(string.Empty, "Nevalidní pokus o přihlášení.");
        return View(model);
    }
    
    // GET pro /Account/Register
    // Zobrazí formulář pro registrování
    public IActionResult Register() {
        return View();
    }
    
    // POST pro /Account/Register
    // Pokud uživatel zadal validní data, vytvoří nového uživatele a vloží jej do databáze.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model) {
        if (ModelState.IsValid) {
            if (await HandleUniqueness(model)) {
                return View(model);
            }
            
            var user = new User { FirstName = model.FirstName, LastName = model.LastName, Age = model.Age, UserName = model.Username, Email = model.Email, DateRegistered = DateTime.Now };
            
            if (model.Avatar != null && model.Avatar.Length > 0) {
                using (var stream = new MemoryStream()) {
                    await model.Avatar.CopyToAsync(stream);
                    if (stream.Length < 2 * 1024 * 1024) {
                        user.Avatar = stream.ToArray();
                    }
                    else {
                        ModelState.AddModelError(string.Empty, "Soubor je příliš velký");
                    }
                }
            }
            else {
                var path = Path.Combine(env.WebRootPath, "pictures", "default-avatar.jpg");
                user.Avatar = await System.IO.File.ReadAllBytesAsync(path);
            }
            
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded) {
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Feed", "Home");
            }
            HandlePasswordErrors(result);
        }
        return View(model);
    }

    // Postará se o to, aby zadaná email adresa a username byli unikátní
    private async Task<bool> HandleUniqueness(RegisterViewModel model) {
        var existEmail = await userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existEmail != null) {
            ModelState.AddModelError(string.Empty, "Tahle email adresa už je zaregistrována.");
            return true;
        }
        var existUser = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
        if (existUser != null) {
            ModelState.AddModelError(string.Empty, "Uživatelské jméno je již zabrané.");
            return true;
        }
        return false;
    }

    // Vypíšeme uživateli error hlášku, pokud nesplnil podmínky pro heslo
    private void HandlePasswordErrors(IdentityResult result) {
        bool errorOccurred = false;
        foreach (var error in result.Errors) {
            if (error.Code == "PasswordTooShort" ||
                error.Code == "PasswordRequiresDigit" ||
                error.Code == "PasswordRequiresUpper" ||
                error.Code == "PasswordRequiresLower" ||
                error.Code == "PasswordRequiresNonAlphanumeric") {
                errorOccurred = true;
            }
        }
        if (errorOccurred) {
            ModelState.AddModelError(string.Empty, "Heslo musí obsahovat alespoň 5 znaků, 1 velký znak, 1 malý znak a číslici");
        }
    }
    
    // GET pro /Account/Password
    [HttpGet]
    public IActionResult Password() {
        return View();
    }
    
    // POST pro /Account/Password
    // Změní heslo uživatele a vrátí zpět na View s hláškou.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Password(PasswordViewModel model) {
        if (ModelState.IsValid) {
            var user = await userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound();
            }
            
            var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded) {
                TempData["Success"] = "Heslo bylo úspěšně změněno.";
                return View(model);
            }
            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Delete() {
        return View();
    }
    
    // Metoda pro odstranění uživatele z databáze.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirm() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return NotFound();
        }
        
        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded) {
            await signInManager.SignOutAsync();
            return RedirectToAction("Feed", "Home");
        }
        return View("Delete");
    }

    // Odhlásí uživatele
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() {
        await signInManager.SignOutAsync();
        return RedirectToAction("Feed", "Home");
    }
}