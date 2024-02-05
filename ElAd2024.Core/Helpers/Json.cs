using Newtonsoft.Json;

namespace ElAd2024.Core.Helpers;

public static class Json
{
    public static async Task<T> ToObjectAsync<T>(string value)
    {
        return await Task.Run<T>(() =>
        {
#pragma warning disable CS8603 // Możliwe zwrócenie odwołania o wartości null.
            return JsonConvert.DeserializeObject<T>(value);
#pragma warning restore CS8603 // Możliwe zwrócenie odwołania o wartości null.
        });
    }

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run(() =>
        {
            return JsonConvert.SerializeObject(value);
        });
    }
}
