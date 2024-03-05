using System.ComponentModel.DataAnnotations.Schema;
using ElAd2024.Converters;

namespace ElAd2024.Models.Database;
public class Photo : TestChildBaseTable
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    private readonly ImageToFullPathConverter converter = new();
    [NotMapped]
    public string? ImageSource => converter.Convert(FileName, typeof(string), new object(), string.Empty)?.ToString();
}