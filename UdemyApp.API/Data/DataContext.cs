using Microsoft.EntityFrameworkCore;
using UdemyApp.API.Models;

namespace UdemyApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext (DbContextOptions<DataContext> options) : base(options){}

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
           
    }
}