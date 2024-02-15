using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.Contracts.Services;
public interface IDatabaseService
{
    string DbPath { get; }
    DbContext Context { get; }

    DbSet<Algorithm> Algorithms { get; set; }
    DbSet<AlgorithmStep> AlgorithmSteps { get; set; }
    DbSet<Step> Steps { get; set; }

    DbSet<Batch> Batches { get; set; }
    DbSet<Test> Tests { get; set; }
    DbSet<Photo> Photos { get; set; }
    DbSet<Voltage> Voltages { get; set; }
    DbSet<Weight>? Weights { get; set; }
    DbSet<Humidity>? Humidities { get; set; }
    DbSet<Temperature>? Temperatures { get; set; }
    DbSet<ElectroStatic>? ElectroStatics { get; set; }


    Task InitializeAsync();
}
