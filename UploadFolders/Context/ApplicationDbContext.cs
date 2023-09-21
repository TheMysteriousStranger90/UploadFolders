using Microsoft.EntityFrameworkCore;
using UploadFolders.Models;

namespace UploadFolders.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Folder> Folders { get; set; }
}