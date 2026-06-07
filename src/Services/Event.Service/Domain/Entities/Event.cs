namespace Event.Service.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public string Location { get; private set; }
    public int AvailableSeats { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Event() { }

    public Event(string title, string description, DateTime date, string location, int availableSeats, decimal price)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Date = date;
        Location = location;
        AvailableSeats = availableSeats;
        Price = price;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, DateTime date, string location, int availableSeats, decimal price)
    {
        Title = title;
        Description = description;
        Date = date;
        Location = location;
        AvailableSeats = availableSeats;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ReserveSeats(int count)
    {
        if (AvailableSeats < count) return false;
        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count)
    {
        AvailableSeats += count;
    }
}
