using System;

public class Bus
{
    public Guid Id { get; set; }
    public int Capacity { get; set; }

    public static Bus CreateNew()
    {
        var rand = new Random();
        return new Bus
        {
            Id = Guid.NewGuid(),
            Capacity = rand.Next(10, 100)
        };
    }
}