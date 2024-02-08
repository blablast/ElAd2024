using Microsoft.VisualBasic;

namespace ElAd2024.Core.Models;
public class HVPlot
{
    // Elapsed time since test start [0.1 seconds]
    public long ElapsedTime { get; set; }

    public byte PhaseNumber { get; set; }

    // High voltage [Volts]
    public int HighVoltage { get; set; }
    public int? HighVoltagePhase1 { get; set; } = null;
    public int? HighVoltagePhase2 { get; set; } = null;
    public int? HighVoltagePhase3 { get; set; } = null;
    public int? HighVoltagePhase4 { get; set; } = null;
}