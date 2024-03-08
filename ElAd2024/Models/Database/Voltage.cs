namespace ElAd2024.Models.Database;
public class Voltage: TestChildBaseTable
{
    public byte Phase { get; set; }

    // Elapsed time since test start [0.1 seconds]
    public int Elapsed { get; set; }
    public int? Value { get; set; }
}
