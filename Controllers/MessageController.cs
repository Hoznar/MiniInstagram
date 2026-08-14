using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using net08.Data;
using net08.Models;
using net08.Models.ViewModels;

namespace net08.Controllers;

public class MessageController : Controller {
    private readonly ILogger<MessageController> _logger;
    private readonly ApplicationDbContext context;

    public MessageController(ILogger<MessageController> logger, ApplicationDbContext context) {
        _logger = logger;
        this.context = context;
    }

    // GET pro Message/Show/id
    // Metoda sesbírá všechny uživatele se kterými přihlášený uživatel zahájil konverzaci.
    // Dále všechny zprávi mezi přihlášeným uživatele a zvoleným uživatelem.
    [HttpGet]
    public async Task<IActionResult> Show(int selectedId = -1) {
        var user = await context.Users
            .Include(user => user.SentMessages)
            .Include(user => user.ReceivedMessages)
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null) {
            return NotFound();
        }

        var messaged = GetMessagedUsers(user);
        List<Message> messages = [];
        if (selectedId != -1) {
            messages = GetMessagesBetweenUsers(user,selectedId); 
        }

        var model = new MessageViewModel() {
            SelectedId = selectedId,
            Users = messaged.Where(u => u != null).ToList(),
            Messages = messages
        };
        
        return View(model);
    }

    private IEnumerable<User> GetMessagedUsers(User user) {
        return context.Messages
            .Where(m => (m.SenderId == user.Id || m.ReceiverId == user.Id))
            .Select(m => m.SenderId == user.Id ? m.Receiver : m.Sender)
            .Distinct()
            .ToList().Distinct();
    }
    
    private List<Message> GetMessagesBetweenUsers(User user, int selectedId) {
        return context.Messages
            .Where(m => (m.SenderId == user.Id && m.ReceiverId == selectedId)
                        || (m.SenderId == selectedId && m.ReceiverId == user.Id))
            .Distinct()
            .OrderBy(m => m.Date)
            .ToList();
    }

    
    // POST pro /Message/Show/id
    // Metoda vytvoří novou zprávu mezi uživateli a uloží ji do databáze
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(MessageViewModel model) {
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        if (user == null) {
            return NotFound();
        }
        
        var message = new Message() {
            SenderId = user.Id,
            ReceiverId = model.SelectedId,
            Text = model.Text,
            Date = DateTime.Now,
        };
        
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        
        return RedirectToAction("Show", "Message", new {selectedId = model.SelectedId});
    }
}