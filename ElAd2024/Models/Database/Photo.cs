namespace ElAd2024.Models.Database;
public class Photo : TestChildBaseTable
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}