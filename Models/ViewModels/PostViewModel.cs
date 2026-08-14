using System.ComponentModel.DataAnnotations;
using net08.Models;

namespace net08.Models.ViewModels;

// ViewModel pro zobrazování příspěvků, jeho autora, a všech jeho komentářů
public class PostViewModel {
    public Post Post { get; set; }
    public User User { get; set; }
    public IEnumerable<Comment> Comments { get; set; }
    
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Text nesmí být prázdný")]
    public string NewComment { get; set; }
}