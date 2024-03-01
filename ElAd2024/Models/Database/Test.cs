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
    public int LoadForce { get; set; }
    public int Phase1Value { get; set; }
    public int Phase1Duration { get; set; }
    public int Phase2Duration { get; set; }
    public int Phase3Value { get; set; }
    public int Phase3Duration { get; set; }
    public int DurationPhaseObserving { get; set; }
    public bool IsPlusPolarity { get; set; }
    public bool AutoRegulation { get; set; }

    public int BatchId { get; set; }
    public Batch Batch { get; set; } = default!;

    public virtual ICollection<Temperature> Temperatures { get; set; } = [];
    public virtual ICollection<Humidity> Humidities { get; set; } = [];
    public virtual ICollection<ElectroStatic> ElectroStatics { get; set; } = [];
    public virtual ICollection<Voltage> Voltages { get; set; } = [];
    public virtual ICollection<Photo> Photos { get; set; } = [];
    public virtual ICollection<Video> Videos { get; set; } = [];
    public virtual ICollection<Weight> Weights { get; set; } = [];
    public ICollection<TestStep> TestSteps { get; set; } = [];

    [NotMapped]
    public double Humidity => (Humidities.Count == 0) ? 0 : (double)(Humidities.FirstOrDefault()?.Value ?? 0.0);   
    public double Temperature => (Temperatures.Count == 0) ? 0 : (double)(Temperatures.FirstOrDefault()?.Value ?? 0.0);

    [NotMapped] public int EndOfPhase1 => Phase1Duration / 100;
    [NotMapped] public int EndOfPhase2 => EndOfPhase1 + Phase2Duration / 100;
    [NotMapped] public int EndOfPhase3 => EndOfPhase2 + Phase3Duration / 100;
    [NotMapped] public int MaxVoltage => IsPlusPolarity ? Phase1Value : Phase3Value;
    [NotMapped] public int MinVoltage => IsPlusPolarity ? -Phase3Value : -Phase1Value;

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