using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using net08.Data;
using net08.Models;
using net08.Models.ViewModels;

namespace net08.Controllers;

[Authorize]
public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) {
        _logger = logger;
        this.context = context;
    }

    // GET pro Feed
    // Defaultně zobrazí všechny příspěvky seřazené podle datumu vytvoření.
    // Další možností je zobrazit pouze příspěvky od sledovaných uživatelů.
    public IActionResult Feed(string type = "all") {
        List<PostViewModel> posts;
        posts = type == "follow" ? GetFollowedPosts() : GetAllPosts();
        return View(posts);
    }

    // Sesbíráme všechny příspěvky včetně autora, počtu lajků a komentářů
    private List<PostViewModel> GetAllPosts() {
        return context.Posts
            .Include(p => p.User)
            .Include(p => p.LikedBy)
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => new PostViewModel() {
                Post = p,
                User = p.User,
                Comments = p.Comments
            }).ToList();
    }

    // Sesbíráme všechny příspěvky od sledovaných uživatelů včetně dalších potřebných informací
    private List<PostViewModel> GetFollowedPosts() {
        var user = context.Users
            .Include(u => u.Following)
            .ThenInclude(u => u.Posts)
            .ThenInclude(p => p.Comments)
            .First(u => u.UserName == User.Identity.Name);

        var posts = user.Following
            .SelectMany(u => u.Posts)
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => new PostViewModel() {
                Post = p,
                User = p.User,
                Comments = p.Comments
            }).ToList();
        return posts;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}