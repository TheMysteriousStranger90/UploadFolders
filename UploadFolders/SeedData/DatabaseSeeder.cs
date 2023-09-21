using UploadFolders.Context;
using UploadFolders.Models;

namespace UploadFolders.SeedData;

public static class DatabaseSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (!context.Folders.Any())
        {
            var rootDirectory = new Folder { Name = "Creating Digital Images" };
            context.Folders.Add(rootDirectory);
            context.SaveChanges();

            var resourcesDirectory = new Folder { Name = "Resources", ParentId = rootDirectory.Id };
            var evidenceDirectory = new Folder { Name = "Evidence", ParentId = rootDirectory.Id };
            var graphicProductsDirectory = new Folder { Name = "Graphic Products", ParentId = rootDirectory.Id };

            context.Folders.AddRange(resourcesDirectory, evidenceDirectory, graphicProductsDirectory);
            context.SaveChanges();

            var primarySourcesDirectory = new Folder { Name = "Primary Sources", ParentId = resourcesDirectory.Id };
            var secondarySourcesDirectory = new Folder { Name = "Secondary Sources", ParentId = resourcesDirectory.Id };

            context.Folders.AddRange(primarySourcesDirectory, secondarySourcesDirectory);
            context.SaveChanges();

            var processDirectory = new Folder { Name = "Process", ParentId = graphicProductsDirectory.Id };
            var finalProductDirectory = new Folder { Name = "Final Product", ParentId = graphicProductsDirectory.Id };

            context.Folders.AddRange(processDirectory, finalProductDirectory);
            context.SaveChanges();
        }
    }
}