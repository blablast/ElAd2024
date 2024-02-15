using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElAd2024.Models.Database;
public class AlgorithmStep
{
    public int Id { get; set; }

    public int AlgorithmId { get; set; }
    public Algorithm Algorithm { get; set; } = default!;
    public int StepId { get; set; }
    public Step Step { get; set; } = default!;

    public int Order { get; set; } // To maintain the order of steps in an algorithm
    public string ActionParameter { get; set; } = string.Empty;
    public string FrontName { get; set; } = string.Empty;
    public string BackName { get; set; } = string.Empty;
}
