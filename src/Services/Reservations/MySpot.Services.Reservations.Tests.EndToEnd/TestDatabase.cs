using System;
using System.Diagnostics.CodeAnalysis;
using Micro.DAL.Postgres;
using Micro.Testing;
using Microsoft.EntityFrameworkCore;
using MySpot.Services.Reservations.Infrastructure.DAL;

namespace MySpot.Services.Reservations.Tests.EndToEnd;

[ExcludeFromCodeCoverage]
internal sealed class TestDatabase : IDisposable
{
    public ReservationsDbContext Context { get; }

    public TestDatabase()
    {
        var options = new OptionsProvider().Get<PostgresOptions>("postgres");
        Context = new ReservationsDbContext(new DbContextOptionsBuilder<ReservationsDbContext>().UseNpgsql(options.ConnectionString).Options);
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}