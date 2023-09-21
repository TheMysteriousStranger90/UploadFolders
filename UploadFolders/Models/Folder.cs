namespace UploadFolders.Models;

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentId { get; set; }
    public Folder Parent { get; set; }
    public List<Folder> Children { get; set; }
}