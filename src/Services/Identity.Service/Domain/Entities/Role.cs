namespace Identity.Service.Domain.Entities;

public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private Role() { }

    public Role(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}
