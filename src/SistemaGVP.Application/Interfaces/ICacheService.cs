namespace SistemaGVP.Application.Interfaces;

public interface ICacheService
{
    T? GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
    void Remove(string key);
    void RemoveByPrefix(string prefix);
}
