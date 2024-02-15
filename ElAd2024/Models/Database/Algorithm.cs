namespace ElAd2024.Models.Database;
public class Algorithm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsBaseAlgorithm { get; init; } = false;
    public ICollection<AlgorithmStep> AlgorithmSteps { get; set; } = [];
}