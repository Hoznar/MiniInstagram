namespace net08.Models.ViewModels;

public class MessageViewModel {
    public int SelectedId { get; set; }
    public IEnumerable<User> Users { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public string Text { get; set; }
}