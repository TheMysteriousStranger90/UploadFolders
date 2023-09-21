using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using UploadFolders.Context;
using UploadFolders.Helpers;
using UploadFolders.Models;

namespace UploadFolders.Controllers;

public class UploadController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public UploadController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public IActionResult ExportFolderStructureToExcel()
    {
        var folders = _context.Folders.ToList();
    
        var exporter = new FolderExporter();
        var excelData = exporter.ExportToExcel(folders);
        
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FolderStructure.xlsx");
    }
    
    public IActionResult UploadFolderFromExcelFile(IFormFile file)
{
    try
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select an Excel file to upload.");
        }
        
        if (!file.FileName.EndsWith(".xlsx"))
        {
            ModelState.AddModelError(string.Empty, "Please upload a valid Excel file (.xlsx).");
        }
        
        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];

                List<Folder> directories = new List<Folder>();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var name = worksheet.Cells[row, 1].Value?.ToString();
                    var parentName = worksheet.Cells[row, 2].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        break;
                    }

                    var directory = new Folder
                    {
                        Name = name,
                        Parent = null,
                        Children = new List<Folder>()
                    };
                    
                    var parentDirectory = directories.FirstOrDefault(d => d.Name == parentName);
                    if (parentDirectory != null)
                    {
                        directory.Parent = parentDirectory;
                        parentDirectory.Children ??= new List<Folder>();
                        parentDirectory.Children.Add(directory);
                    }

                    directories.Add(directory);
                }

                ChangeFoldersInDatabase(directories);
            }
        }

        return RedirectToAction("Index", "Home");
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
        return View();
    }
}
    
    public IActionResult UploadFolderFromSystemDir()
    {
        return View(new UploadFolder());
    }

    [HttpPost]
    public IActionResult UploadFolderFromSystemDir(UploadFolder file)
    {
        try
        {
            if (string.IsNullOrEmpty(file.Path))
            {
                file.Path = "D:";
            }

            var rootPath = file.Path.EndsWith("\\") ? file.Path : $"{file.Path}\\";
            var currentFolderName = file.Name;

            List<Folder> directories = new();

            var currentRootPath = $"{rootPath}{currentFolderName}";

            if (Directory.Exists(rootPath))
            {
                directories = AddChildDirectories(directories, null, currentRootPath, rootPath);

                ChangeFoldersInDatabase(directories);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogError("Specified directory does not exist.");
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while uploading folders.");
            return View("Error");
        }
    }

    private List<Folder> AddChildDirectories(List<Folder> directories, Folder parentDirectory, string currentPath, string rootPath)
    {
        var directoryInfo = new DirectoryInfo(currentPath);

        var currentDirectory = new Folder
        {
            Name = directoryInfo.Name,
            ParentId = parentDirectory?.Id,
            Parent = parentDirectory,
            Children = new List<Folder>()
        };

        if (parentDirectory != null)
        {
            parentDirectory.Children ??= new List<Folder>();
            parentDirectory.Children.Add(currentDirectory);
        }

        directories.Add(currentDirectory);

        foreach (var subDirectory in directoryInfo.GetDirectories())
        {
            AddChildDirectories(directories, currentDirectory, subDirectory.FullName, rootPath);
        }

        return directories;
    }


    private void ChangeFoldersInDatabase(List<Folder> directories)
    {
        if (_context.Folders.Count() != 0)
        {
            _context.Folders.RemoveRange(_context.Folders);
        }

        _context.Folders.AddRange(directories);
        _context.SaveChanges();
    }
}