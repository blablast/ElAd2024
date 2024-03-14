namespace ElAd2024.Models.Database;
public class ElectroStatic : TestValueBaseTable<int>
{
    // Elapsed time since test start [0.1 seconds]
    
    public string Description {  get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
