using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models;
using Microsoft.EntityFrameworkCore;
using Windows.UI;
using static System.Net.Mime.MediaTypeNames;

namespace ElAd2024.Services;
[ObservableObject]
public partial class DatabaseService : DbContext, IDatabaseService
{
    public DbContext Context => this;
    public string DBPath { get; }
    [ObservableProperty] private DbSet<Batch>? batches;
    [ObservableProperty] private DbSet<Test>? tests;
    [ObservableProperty] private DbSet<Voltage>? voltages;
    [ObservableProperty] private DbSet<Photo>? photos;
    public DatabaseService()
    {
        DBPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ElAd2024.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
             .UseSqlite($"DataSource={DBPath}")
             .EnableSensitiveDataLogging();
    }

    public async Task InitializeAsync()
    {
        try
        {
            //await Database.EnsureDeletedAsync();
            await Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        await Task.CompletedTask;
    }
}
