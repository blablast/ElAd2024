namespace ElAd2024.Models.Database;
public class Batch
{
    public int Id { get; set; }

    public string Name { get; set; } = $"New Batch";

    public string Description { get; set; } = string.Empty;

    public string FabricType { get; set; } = string.Empty;

    public string FabricComposition { get; set; } = string.Empty;

    public string FabricColor { get; set; } = string.Empty;

    public int FabricGSM { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = [];

    public override string ToString() => $"{Name} - {Tests?.Count} test{(Tests?.Count > 1 ? "s" : "")}";
}