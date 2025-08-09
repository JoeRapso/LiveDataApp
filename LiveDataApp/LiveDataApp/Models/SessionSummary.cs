namespace LiveDataApp.Models;

public record SessionSummary(Guid SessionId, 
    string Series, 
    string Name, 
    string Track, 
    string State, 
    DateTime StartTime, 
    TimeSpan Duration);