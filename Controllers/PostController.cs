using System.Net.Mime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using net08.Models;
using net08.Data;
using net08.Models.ViewModels;

namespace net08.Controllers;

public class PostController : Controller {
    private readonly ILogger<PostController> _logger;
    private readonly ApplicationDbContext context;

    public PostController(ILogger<PostController> logger, ApplicationDbContext context) {
        _logger = logger;
        this.context = context;
    }

    // GET pro /Post/Show/id
    // Metoda pro zobrazení konkrétního příspěvku. Vyhledáme přspěvěk podle id.
    // Následně všechny potřebné informace přichistáme do ViewModelu
    [HttpGet]
    public async Task<IActionResult> Show(int id) {
        var post = await context.Posts
            .Include(p => p.User)
            .Include(p => p.LikedBy)
            //Potřeba includovat autora pro zobrazení jména u každého kometáře
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            // Potřeba includovat Liky pro zobrazení počtu lajků u komentáře
            .Include(p => p.Comments)
                .ThenInclude(c => c.LikedBy)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) {
            return NotFound();
        }

        var model = new PostViewModel() {
            Post = post,
            User = post.User,
            Comments = post.Comments.OrderByDescending(c => c.DateCreated)
        };
        return View(model);
    }

    // GET pro /Post/Edit/id
    // Metoda nachystá informace o příspěvku a zobrazí formulář pro úpravu příspěvku.
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) {
            return NotFound();
        }
        
        var model = new NewPostViewModel() {
            Id = id,
            Text = post.Text + " " + string.Join(" ", post.Tags.Select(tag => tag))
        };
        return View(model);
    }
    
    // POST pro /Post/Edit/id
    // Metoda zpracuje upravený text a uloží změny do databáze
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(NewPostViewModel model) {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == model.Id);
        if (post == null) {
            return NotFound();
        }

        post.Text = model.Text;
        post = ProcessTags(post);
        context.Update(post);
        await context.SaveChangesAsync();

        return RedirectToAction("Show", "Post", new { id = post.Id });
    }
    
    // POST pro /Post/Delete/id
    // Metoda jednoduše odstraní Post z databáze, a vrátíme uživatele na domovskou stránku
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) {
            return NotFound();
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return RedirectToAction("Feed", "Home");
    }

    // POST pro /Post/Like/id
    // Metoda zajistí že se do tabulky lajků určitého postu přida lajk od uživatele
    // Pokud tam už lajk je, odstraní ho
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int id) {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        var post = await context.Posts
            .Include(post => post.LikedBy)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (user == null || post == null) {
            return NotFound();
        }

        var like = post.LikedBy.FirstOrDefault(pl => pl.UserId == user.Id);
        if (like == null) {
            var pl = new PostLike() {
                Post = post,
                User = user
            };
            context.PostLikes.Add(pl);
        }
        else {
            context.PostLikes.Remove(like);
        }
        
        await context.SaveChangesAsync();
        return RedirectToAction("Show", "Post", new { id = post.Id});
    }
    
    // POST pro /Post/LikeComment/id
    // Stejné jako předchozí metoda, ale pro lajkování komentářů
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LikeComment(int id) {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        var comment = await context.Comments
            .Include(c => c.LikedBy)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (user == null || comment == null) {
            return NotFound();
        }

        var like = comment.LikedBy.FirstOrDefault(cl => cl.UserId == user.Id);
        if (like == null) {
            var cl = new CommentLike() {
                Comment = comment,
                User = user
            };
            context.CommentLikes.Add(cl);
        }
        else {
            context.CommentLikes.Remove(like);
        }
        
        await context.SaveChangesAsync();
        return RedirectToAction("Show", "Post", new { id = comment.Post.Id });
    }

    // POST pro /Post/Comment
    // Metoda získá nový komentář přes ViewModel, zjistíme uživatele a post
    // a vložíme do databáze.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(PostViewModel model) {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == model.Id);
        if (user == null || post == null) {
            return NotFound();
        }

        var comment = new Comment() {
            Text = model.NewComment,
            Post = post,
            Author = user,
            DateCreated = DateTime.Now,
        };
        
        await context.Comments.AddAsync(comment);
        await context.SaveChangesAsync();
        
        return RedirectToAction("Show", new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int id) {
        var comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment == null) {
            return NotFound();
        }
        
        context.Comments.Remove(comment);
        await context.SaveChangesAsync();
        
        return RedirectToAction("Show", "Post", new { id = comment.PostId});
    }

    // Metoda která vrací View pro vytvoření nového příspěvku
    public IActionResult Create() {
        return View();
    }
    
    // Metoda zpracuje informace z formuláře z View.
    // Vytvoří nový příspěvek, uloží ho do databáze a zobrazí ho.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewPostViewModel model) {
        if (ModelState.IsValid) {
            var user = context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null) {
                return NotFound();
            }
            
            Post? post = await CreatePost(model, user);
            // Pokud je hodnota null, obrázek je příliš velký
            if (post == null) {
                return View();
            }
            
            context.Posts.Add(post);
            await context.SaveChangesAsync();
            return RedirectToAction("Show", new { id = post.Id });
        }
        return View(model);
    }

    // Vytváření příspěvku. Zpracujeme obrázek a získáme tagy pro samostatné zobrazení.
    // Pokud je obrázek příliš velký, vrátíme null, abychom mohli vyhodit chybu.
    private async Task<Post?> CreatePost(NewPostViewModel model, User user) {
        Post post = new Post {
            User = user,
            Text = model.Text,
            ReleaseDate = DateTime.Now
        };

        if (model.Image.Length > 0) {
            using (var stream = new MemoryStream()) {
                await model.Image.CopyToAsync(stream);
                // Obrázek může být max 2MB
                if (stream.Length < 2 * 1024 * 1024) {
                    post.Image = stream.ToArray();
                }
                else {
                    ModelState.AddModelError(string.Empty, "Soubor je příliš velký");
                    return null;
                }
            }
        }
        else {
            post.Image = await System.IO.File.ReadAllBytesAsync("~/pictures/default-post.png");
        }

        post = ProcessTags(post);
        return post;
    }

    private Post ProcessTags(Post post) {
        string pattern = @"#\w+";
        var matches = Regex.Matches(post.Text, pattern);
        post.Text = Regex.Replace(post.Text, pattern, string.Empty).Trim();
        post.Tags = Array.ConvertAll(matches.ToArray(), match => match.Value);
        return post;
    }
}