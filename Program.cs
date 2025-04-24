using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var service = new InMemoryCrudService<Bus>(b => b.Id, "buses.json");
        var buses = new ConcurrentBag<Bus>();

        Parallel.For(0, 1000, i =>
        {
            buses.Add(Bus.CreateNew());
        });

        foreach (var bus in buses)
        {
            await service.CreateAsync(bus);
        }

        var capacities = buses.Select(b => b.Capacity).ToList();
        Console.WriteLine($"Min: {capacities.Min()} | Max: {capacities.Max()} | Avg: {capacities.Average()}");

        await service.SaveAsync();

        // Sync primitives
        object lockObj = new();
        Semaphore semaphore = new(1, 1);
        AutoResetEvent autoReset = new(false);

        lock (lockObj)
        {
            Console.WriteLine("Lock example executed.");
        }

        semaphore.WaitOne();
        Console.WriteLine("Semaphore example executed.");
        semaphore.Release();

        Task.Run(() =>
        {
            Thread.Sleep(1000);
            autoReset.Set();
        });
        autoReset.WaitOne();
        Console.WriteLine("AutoResetEvent triggered.");
    }
}