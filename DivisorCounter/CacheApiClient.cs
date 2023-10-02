namespace DivisorCounter;

public class CacheApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://cache-service:8001/cache";


    public CacheApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int?> GetDivisorCount(long number)
    {
        var response = await _httpClient.GetAsync($"{BaseUrl}?number={number}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsAsync<int>();
        return null;
    }

    public async Task SetDivisorCount(long number, int divisorCounter)
    {
        var response = await _httpClient.PostAsync($"{BaseUrl}?number={number}&divisorCounter={divisorCounter}", null);
        response.EnsureSuccessStatusCode();
    }
}