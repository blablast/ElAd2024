using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElAd2024.Models.Database;
public class Test
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public float Humidity { get; set; }
    public float Temperature { get; set; }
    public int LoadForce { get; set; }
    public int HVPhaseCharging { get; set; }
    public int DurationPhaseCharging { get; set; }
    public int DurationPhaseIntermediary { get; set; }
    public int HVPhaseLoading { get; set; }
    public int DurationPhaseLoading { get; set; }
    public int DurationPhaseObserving { get; set; }
    public bool IsPlusPolarity { get; set; }
    public bool AutoRegulation { get; set; }
    public string VideoPath { get; set; } = string.Empty;

    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;

    public virtual ICollection<Temperature> Temperatures { get; set; } = [];
    public virtual ICollection<Humidity> Humidities { get; set; } = [];
    public virtual ICollection<ElectroStatic> ElectroStatics { get; set; } = [];
    public virtual ICollection<Voltage> Voltages { get; set; } = [];
    public virtual ICollection<Photo> Photos { get; set; } = [];
    public virtual ICollection<Weight> Weights { get; set; } = [];
    public ICollection<TestStep> TestSteps { get; set; } = [];

    [NotMapped] public int EndOfPhase1 => DurationPhaseCharging / 100;
    [NotMapped] public int EndOfPhase2 => EndOfPhase1 + DurationPhaseIntermediary / 100;
    [NotMapped] public int EndOfPhase3 => EndOfPhase2 + DurationPhaseLoading / 100;
    [NotMapped] public int MaxVoltage => IsPlusPolarity ? HVPhaseCharging : HVPhaseLoading;
    [NotMapped] public int MinVoltage => IsPlusPolarity ? -HVPhaseLoading : -HVPhaseCharging;

    [NotMapped]
    public string WeightsToString
    {
        get
        {
            StringBuilder result = new();
            Weights.ToList().ForEach(weight => result.Append($"{weight.Value}g ({weight.Description})\n"));
            return result.ToString();
        }
    }


}