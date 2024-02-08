using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Windows.Networking;
using Windows.Storage;

namespace ElAd2024.Models;

#pragma warning disable CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.

public class Batch
{
    public int BatchId
    {
        get; set;
    }

    public string Name { get; set; } = $"New Batch";

    public string Description { get; set; } = string.Empty;

    public string FabricType { get; set; } = string.Empty;

    public string FabricComposition { get; set; } = string.Empty;

    public string FabricColor { get; set; } = string.Empty;

    public int FabricGSM
    {
        get; set;
    }

    public virtual List<Test> Tests { get; set; } = [];

    public override string ToString()
        => $"{Name} - {Tests?.Count} test{(Tests?.Count > 1 ? "s" : "")}";
}
public class Test
{
    // Test ID
    public int TestId
    {
        get; set;
    }

    // Test name
    public string Name { get; set; } = $"New Test";

    public DateTime Date
    {
        get; set;
    }

    // Environment humidity [%]
    public float Humidity
    {
        get; set;
    }

    // Environment temperature [Celsius]
    public float Temperature
    {
        get; set;
    }

    // Robot force used in SkipTouch [N]
    public int LoadForce
    {
        get; set;
    }

    // Voltage in Charging phase [Volts]
    public int HVPhaseCharging
    {
        get; set;
    }

    // Duration of Charging phase [0.1 seconds]
    public int DurationPhaseCharging
    {
        get; set;
    }

    // Duration of Intermediate phase [0.1 seconds]
    public int DurationPhaseIntermediary
    {
        get; set;
    }

    // Voltage in Loading phase [Volts]
    public int HVPhaseLoading
    {
        get; set;
    }

    // Duration of Loading phase [0.1 seconds]
    public int DurationPhaseLoading
    {
        get; set;
    }

    public int DurationPhaseObserving
    {
        get; set;
    }

    public bool IsPlusPolarity
    {
        get; set;
    }


    public bool AutoRegulation
    {
        get; set;
    }

    public string VideoPath { get; set; } = string.Empty;

    public int BatchId
    {
        get; set;
    }
    public Batch Batch
    {
        get; set;
    }
    public virtual List<Voltage> Voltages { get; set; } = [];
    public virtual List<Photo> Photos { get; set; } = [];
    public virtual List<Weight> Weights { get; set; } = [];

    [NotMapped]
    public string WeightsToString
    {
        get
        {
            StringBuilder result = new();
            foreach (var weight in Weights)
            {
                result.Append($"{weight.Value}g ({weight.Description})\n");
            }
            return result.ToString();
        }
    }


}

public class Voltage
{
    public int VoltageId
    {
        get; set;
    }

    // Phase number [1 to 4]
    // 1 - Charging
    // 2 - Intermediary
    // 3 - Loading
    // 4 - Loaded
    public byte PhaseNumber
    {
        get; set;
    }

    // Elapsed time since test start [0.1 seconds]
    public long ElapsedTime
    {
        get; set;
    }

    // High voltage [Volts]
    public int HighVoltage
    {
        get; set;
    }

    public int TestId
    {
        get; set;
    }
    public Test Test
    {
        get; set;
    }
}

public class Photo
{
    public int PhotoId
    {
        get; set;
    }

    public string FileName { get; set; } = string.Empty;
    public string FullPathFileName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TestId
    {
        get; set;
    }

    public Test Test
    {
        get; set;
    }
}

public class Weight
{
    public int WeightId
    {
        get; set;
    }

    public int Value
    {
        get; set;
    }

    public string Description { get; set; } = string.Empty;

    public int TestId
    {
        get; set;
    }

    public Test Test
    {
        get; set;
    }
}

#pragma warning restore CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
