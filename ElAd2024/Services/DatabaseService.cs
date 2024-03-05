using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Models.Database;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;

namespace ElAd2024.Services;

[ObservableObject]
public partial class DatabaseService : DbContext, IDatabaseService
{
    [ObservableProperty] private DbSet<Algorithm>? algorithms;
    [ObservableProperty] private DbSet<AlgorithmStep>? algorithmSteps;
    [ObservableProperty] private DbSet<Step>? steps;

    [ObservableProperty] private DbSet<Batch>? batches;

    [ObservableProperty] private DbSet<Test>? tests;
    [ObservableProperty] private DbSet<Photo>? photos;
    [ObservableProperty] private DbSet<Voltage>? voltages;
    [ObservableProperty] private DbSet<Weight>? weights;
    [ObservableProperty] private DbSet<Humidity>? humidities;
    [ObservableProperty] private DbSet<Temperature>? temperatures;
    [ObservableProperty] private DbSet<ElectroStatic>? electroStatics;

    public DbContext Context => this;

    public string DbPath { get; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ElAd2024.db");

    public async Task InitializeAsync()
    {
        
        // Copy database to known location
        var dbFile = await StorageFile.GetFileFromPathAsync(DbPath);
        if (dbFile is null)
        {
                   var dbUri = new Uri("ms-appx:///ElAd2024.db");
                   var dbFileInPackage = await StorageFile.GetFileFromApplicationUriAsync(dbUri);
                   await dbFileInPackage.CopyAsync(ApplicationData.Current.LocalFolder, "ElAd2024.db", NameCollisionOption.ReplaceExisting);
         }


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

        if (Steps?.ToList().Count == 0)
        {
            await InitializeSteps();
        }

        if (Algorithms?.ToList().Count == 0 && Steps is not null)
        {
            await InitializeAlgorithms();
        }

        Debug.WriteLine($"{DbPath}");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder
            .UseSqlite($"DataSource={DbPath}")
            .EnableSensitiveDataLogging();
            //.LogTo(s => Debug.WriteLine(s));

    protected override void OnModelCreating(ModelBuilder modelBuilder) => base.OnModelCreating(modelBuilder);
    // If needed, configure the Order column here as well

    private async Task InitializeSteps()
    {
        Steps?.AddRange(new ObservableCollection<Step>
            {
                new() { AsyncActionName = "Start", Style = Step.DeviceType.Computer, IsMoveable = false, IsFirst = true, IsMandatory = true },
                new() { AsyncActionName = "Finish", Style = Step.DeviceType.Computer, IsMoveable = false, IsLast = true, IsMandatory = true },
                new() { AsyncActionName = "GetPhoto", Style = Step.DeviceType.Computer, HasParameter = true },
                new() { AsyncActionName = "GetWeight", Style = Step.DeviceType.Scale, HasParameter = true },
                new() { AsyncActionName = "RobotMove", Style = Step.DeviceType.Robot, HasParameter = true, IsNumericParameter = true },
                new() { AsyncActionName = "GetTemperature", Style = Step.DeviceType.Environment },
                new() { AsyncActionName = "GetHumidity", Style = Step.DeviceType.Environment },
                new() { AsyncActionName = "GetStatic", Style = Step.DeviceType.Environment },
                new() { AsyncActionName = "TakeFabric", Style = Step.DeviceType.Pad },
                new() { AsyncActionName = "Wait", Style = Step.DeviceType.Computer, HasParameter = true, IsNumericParameter = true },
                new() { AsyncActionName = "ReleaseFabric", Style = Step.DeviceType.Pad }
            }) ;
        await SaveChangesAsync();
    }
    private async Task InitializeAlgorithms()
    {
        ArgumentNullException.ThrowIfNull(Algorithms);
        ArgumentNullException.ThrowIfNull(Steps);

        var dA = new Algorithm { Name = "Default", Description = "Default algorithm", IsBaseAlgorithm = true };
        Algorithms.Add(dA);
        await SaveChangesAsync();

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "Start"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Start",
            BackName = "Setting up!"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetTemperature"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Temperature",
            BackName = "Getting temperature!"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetHumidity"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Humidity",
            BackName = "Getting humidity!"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetPhoto"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Photo",
            BackName = "Taking photo...",
            ActionParameter = "Ready to PickUp"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetWeight"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Weight",
            BackName = "Weighting...",
            ActionParameter = "StackFull"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "RobotMove"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"PickUp",
            BackName = "Moving Pad...",
            ActionParameter = "1"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "TakeFabric"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Take",
            BackName = "Taking..."
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "RobotMove"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"PickUp+",
            BackName = "Moving Pad...",
            ActionParameter = "2"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetWeight"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Weight",
            BackName = "Weighting...",
            ActionParameter = "StackPicked"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "GetPhoto"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Photo",
            BackName = "Taking photo...",
            ActionParameter = "Picked"
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "Wait"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Wait",
            BackName = "Waiting...",
            ActionParameter = "3000"
        });


        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "ReleaseFabric"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Release",
            BackName = "Releasing up\nfabric..."
        });

        dA.AlgorithmSteps.Add(new AlgorithmStep
        {
            Step = Steps.Single(n => n.AsyncActionName == "Finish"),
            Order = dA.AlgorithmSteps.Count,
            FrontName = @"Finish",
            BackName = "Finishing..."
        }
        );

        await SaveChangesAsync();
    }
}