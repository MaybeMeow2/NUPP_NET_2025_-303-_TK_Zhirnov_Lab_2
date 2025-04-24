using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class InMemoryCrudService<T> : ICrudServiceAsync<T> where T : class
{
    private readonly ConcurrentDictionary<Guid, T> _storage = new();
    private readonly Func<T, Guid> _idSelector;
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileSemaphore = new(1, 1);

    public InMemoryCrudService(Func<T, Guid> idSelector, string filePath)
    {
        _idSelector = idSelector;
        _filePath = filePath;
    }

    public async Task<bool> CreateAsync(T element) =>
        _storage.TryAdd(_idSelector(element), element);

    public async Task<T> ReadAsync(Guid id) =>
        _storage.TryGetValue(id, out var element) ? element : null;

    public async Task<IEnumerable<T>> ReadAllAsync() =>
        _storage.Values;

    public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount) =>
        _storage.Values.Skip((page - 1) * amount).Take(amount);

    public async Task<bool> UpdateAsync(T element)
    {
        var id = _idSelector(element);
        if (_storage.ContainsKey(id))
        {
            _storage[id] = element;
            return true;
        }
        return false;
    }

    public async Task<bool> RemoveAsync(T element) =>
        _storage.TryRemove(_idSelector(element), out _);

    public async Task<bool> SaveAsync()
    {
        await _fileSemaphore.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(_storage.Values);
            await File.WriteAllTextAsync(_filePath, json);
            return true;
        }
        finally
        {
            _fileSemaphore.Release();
        }
    }

    public IEnumerator<T> GetEnumerator() => _storage.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}