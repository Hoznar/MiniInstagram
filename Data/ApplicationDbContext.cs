using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using net08.Models;

namespace net08.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base (options) { }
    
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Comments)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId);
        
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Post>()
            .Property(p => p.Tags)
            .HasConversion(
                s => string.Join(',', s),
                s => s.Split(',', StringSplitOptions.RemoveEmptyEntries));
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.Receiver)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Konfigurace propojovacích tabulek
        modelBuilder.Entity<PostLike>()
            .HasKey(pl => pl.Id);
        
        modelBuilder.Entity<CommentLike>()
            .HasKey(cl => cl.Id);
        
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.User)
            .WithMany(u => u.LikedPosts)
            .HasForeignKey(pl => pl.UserId);
        
        modelBuilder.Entity<PostLike>()
            .HasOne(pl => pl.Post)
            .WithMany(p => p.LikedBy)
            .HasForeignKey(pl => pl.PostId);
        
        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.User)
            .WithMany(u => u.LikedComments)
            .HasForeignKey(cl => cl.UserId);
        
        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.Comment)
            .WithMany(c => c.LikedBy)
            .HasForeignKey(cl => cl.CommentId);
    }
}