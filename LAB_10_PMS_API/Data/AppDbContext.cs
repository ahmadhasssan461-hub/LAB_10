using LAB_10_PMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace LAB_10_PMS_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}