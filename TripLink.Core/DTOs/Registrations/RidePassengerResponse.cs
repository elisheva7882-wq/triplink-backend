public class RidePassengerResponse
{
    public int PassengerId { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; }
}