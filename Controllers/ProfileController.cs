using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using net08.Models;
using net08.Data;
using net08.Models.ViewModels;

namespace net08.Controllers;

public class ProfileController : Controller {
    private readonly ILogger<ProfileController> _logger;
    private readonly ApplicationDbContext context;
    private readonly SignInManager<User> signInManager;

    public ProfileController(ILogger<ProfileController> logger, ApplicationDbContext context, SignInManager<User> signInManager) {
        _logger = logger;
        this.context = context;
        this.signInManager = signInManager;
    }
    
    // GET pro /Home/Profile/id
    // Metoda zobrazí profil zvoleného uživatele.
    // Argument type rozhoduje zda se zobrazí příspěvky nebo komentáře.
    [HttpGet]
    public IActionResult Show(int id, string type = "posts") {
        var user = context.Users
            .Include(u => u.Followed)
            .Include(u => u.Following)
            .FirstOrDefault(u => u.Id == id);
        if (user == null) {
            return NotFound();
        }

        var model = FindUserPosts(user);
        
        ViewData["ContentType"] = type == "comments" ? "comments" : "posts";
        return View("Profile",model);
    }

    // Najdeme příspěvky a komentáře určitého uživatele a nachystáme ViewModel
    private ProfileViewModel FindUserPosts(User user) {
        var posts = context.Posts
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.ReleaseDate)
            .ToList();
        
        var comments = context.Comments
            .Where(c => c.AuthorId == user.Id)
            // Abychom mohli vypsat na jakém příspěvku je komentář, musíme includovat posty a autora
            .Include(c => c.Post)
                .ThenInclude(p => p.User)
            .OrderByDescending(c => c.DateCreated)
            .ToList();

        var model = new ProfileViewModel() {
            user = user,
            posts = posts,
            comments = comments
        };
        
        return model;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(int id) {
        var currentUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        var user = await context.Users
            .Include(user => user.Followed)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (currentUser == null || user == null) {
            return NotFound();
        }
        
        if (user.Followed.FirstOrDefault(u => u.Id == currentUser.Id) == null) {
            currentUser.Following.Add(user);
            user.Followed.Add(currentUser);
        }
        else {
            currentUser.Following.Remove(user);
            user.Followed.Remove(currentUser);
        }
        await context.SaveChangesAsync();
        
        return RedirectToAction("Show", "Profile", new {id});
    }

    // GET pro /Profile/Edit
    // Vytvoříme ViewModel pro přihlášeného uživatele a pošleme ho na zobrazené formuláře
    [HttpGet]
    public async Task<IActionResult> Edit() {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null) {
            return NotFound();
        }
        
        var model = new ProfileEditViewModel() {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            Username = user.UserName
        };
        
        return View(model);
    }
    
    // POST pro /Profile/Edit
    // Po vyplnění formuláře aktulizujeme údaje o uživateli a vrátíme ho na jeho profil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model) {
        if (ModelState.IsValid) {
            var existUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username && u.UserName != User.Identity.Name);
            if (existUser != null) {
                ModelState.AddModelError(string.Empty, "Uživatelské jméno je již zabrané.");
                return View(model);
            }
        
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) {
                return NotFound();
            }
            
            if (model.Avatar != null && model.Avatar.Length > 0) {
                using (var stream = new MemoryStream()) {
                    await model.Avatar.CopyToAsync(stream);
                    if (stream.Length < 2 * 1024 * 1024) {
                        user.Avatar = stream.ToArray();
                    }
                    else {
                        ModelState.AddModelError(string.Empty, "Soubor je příliš velký");
                        return View(model);
                    }
                }
            }
        
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Age = model.Age;
            user.UserName = model.Username;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            
            // Jelikož při změně přezdívky uživatele se User.Identity.Name aktulizuje až po přihlášení, relogneme uživatle
            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, isPersistent: false);
            
            return RedirectToAction("Show","Profile", new {id = user.Id});
        }
        return View(model);
    }
}