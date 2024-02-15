namespace ElAd2024.Models.Database;
public class Voltage: TestValueBaseTable<int?>
{
    // Phase number [1 to 4]
    // 1 - Charging
    // 2 - Intermediary
    // 3 - Loading
    // 4 - Loaded
    public byte Phase { get; set; }

    // Elapsed time since test start [0.1 seconds]
    public int Elapsed { get; set; }
}
