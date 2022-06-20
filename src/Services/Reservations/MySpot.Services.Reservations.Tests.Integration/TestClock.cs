using System;
using System.Diagnostics.CodeAnalysis;
using Micro.Time;

namespace MySpot.Services.Reservations.Tests.Integration;

[ExcludeFromCodeCoverage]
public class TestClock : IClock
{
    public DateTime Current()
        => new(2022, 06, 20, 12, 0, 0);
}