namespace net08.Models;

public class Comment {
    public int Id { get; set; }
    
    public int PostId { get; set; }
    public virtual Post Post { get; set; }
    
    public int AuthorId { get; set; }
    public virtual User Author { get; set; }
    
    public string Text { get; set; }
    public DateTime DateCreated { get; set; }
    
    
    public virtual ICollection<CommentLike> LikedBy { get; set; } =  new List<CommentLike>();
}

// Tabulka pro zjištění kdo dal like na jaký komentář
public class CommentLike {
    public int Id { get; set; }
    
    public int CommentId { get; set; }
    public virtual Comment Comment { get; set; }
    
    public int UserId { get; set; }
    public virtual User User { get; set; }
}