using System.Collections.ObjectModel;
using ElAd2024.Models;
using Microsoft.EntityFrameworkCore;

namespace ElAd2024.Contracts.Services;
public interface IDatabaseService
{
    DbContext Context { get; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Voltage> Voltages { get; set; }
    public DbSet<Photo> Photos { get; set; }
    string DBPath { get; }

    Task InitializeAsync();

}
