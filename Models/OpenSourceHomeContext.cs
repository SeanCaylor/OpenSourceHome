using Microsoft.EntityFrameworkCore;

namespace OpenSourceHome.Models
{
    public class OpenSourceHomeContext : DbContext
    {
        public OpenSourceHomeContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<UserPostLike> UserPostLikes { get; set; }
    }
}