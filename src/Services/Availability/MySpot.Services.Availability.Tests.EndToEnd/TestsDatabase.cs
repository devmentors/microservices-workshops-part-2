using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Micro.DAL.Postgres;
using Micro.Testing;
using Microsoft.EntityFrameworkCore;
using MySpot.Services.Availability.Infrastructure.DAL;

namespace MySpot.Services.Availability.Tests.EndToEnd;

[ExcludeFromCodeCoverage]
internal sealed class TestsDatabase : IDisposable
{
    public AvailabilityDbContext Context { get; }

    public TestsDatabase()
    {
        var connectionString = new OptionsProvider().Get<PostgresOptions>("postgres").ConnectionString;
        Context = new AvailabilityDbContext(new DbContextOptionsBuilder<AvailabilityDbContext>().UseNpgsql(connectionString).Options);
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public Task InitAsync() => Context.Database.MigrateAsync();
    
    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}