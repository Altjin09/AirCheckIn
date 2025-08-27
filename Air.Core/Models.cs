namespace Air.Core;

public enum FlightStatus { CheckingIn, Boarding, Departed, Delayed, Cancelled }

public class Flight
{
    public int Id { get; set; }
    public string FlightNo { get; set; } = default!;
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public DateTime ScheduledDeparture { get; set; }
    public string Gate { get; set; } = "A1";
    public FlightStatus Status { get; set; } = FlightStatus.CheckingIn;
    public List<Seat> Seats { get; set; } = new();
    public byte[]? RowVersion { get; set; }
}

public class Seat
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public Flight Flight { get; set; } = default!;
    public string SeatNo { get; set; } = default!;
    public bool IsAssigned { get; set; }
    public int? AssignedPassengerId { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public string? LockedBy { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class Passenger
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string PassportNo { get; set; } = default!;
}

public class Booking
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public int PassengerId { get; set; }
    public string? SeatNoPreferred { get; set; }
    public bool CheckedIn { get; set; }
    public DateTime? CheckedInAtUtc { get; set; }
    public Flight Flight { get; set; } = default!;
    public Passenger Passenger { get; set; } = default!;
}
