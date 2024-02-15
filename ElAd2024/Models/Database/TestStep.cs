namespace ElAd2024.Models.Database;
public class TestStep
{
    public int Id { get; set; }

    public int TestId { get; set; }
    public Test Test { get; set; } = default!;
    public int StepId { get; set; }
    public Step Step { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string ActionParameter { get; set; } = string.Empty;
    public string FrontName { get; set; } = string.Empty;
}
