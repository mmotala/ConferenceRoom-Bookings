public sealed class Room : BaseEntity
{
    public Room(string name, int capacity, string location)
    {
        Name = name;
        Capacity = capacity;
        Location = location;
    }

    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public string Location { get; private set; } = string.Empty;

    public void Update(string name, int capacity, string location)
    {
        Name = name;
        Capacity = capacity;
        Location = location;
    }
}