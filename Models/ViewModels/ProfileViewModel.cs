using net08.Models;

namespace net08.Models.ViewModels;

public class ProfileViewModel {
    public User user { get; set; }
    public IEnumerable<Post> posts { get; set; }
    public IEnumerable<Comment> comments { get; set; }
}