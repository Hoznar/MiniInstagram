using Microsoft.AspNetCore.Identity;

namespace net08.Models;

public class User : IdentityUser<int> {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    
    public byte[]? Avatar { get; set; }
    public DateTime DateRegistered { get; set; }
    
    public virtual List<User> Following { get; set; } = [];
    public virtual List<User> Followed { get; set; } = [];
    
    public virtual ICollection<Post> Posts { get; set; }
    public virtual ICollection<PostLike> LikedPosts { get; set; }
    
    public virtual ICollection<Comment> Comments { get; set; }
    public virtual ICollection<CommentLike> LikedComments { get; set; }
    
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}