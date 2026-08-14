using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace net08.Models;

public class Post {
    public int Id { get; set; }
    public string Text { get; set; }
    public byte[] Image { get; set; }
    public string[] Tags { get; set; }
    [DataType(DataType.Date)]
    [Column(TypeName = "Date")]
    public DateTime ReleaseDate { get; set; }
    
    public int UserId { get; set; }
    public virtual User User { get; set; }
    
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostLike> LikedBy { get; set; } = new List<PostLike>();
}

// Tabulka pro zjištění kdo dal like na jaký post.
public class PostLike {
    public int Id { get; set; }
    
    public int PostId { get; set; }
    public virtual Post Post { get; set; }
    
    public int UserId { get; set; }
    public virtual User User { get; set; }
}