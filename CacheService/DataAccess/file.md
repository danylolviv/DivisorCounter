Here are the files from the DivisorCounter



using System.Diagnostics;

namespace DivisorCounter;

public class Program
{
public static async Task Main()
{
var cacheApiClient = new CacheApiClient(new HttpClient());

        var first = 1_000_000_000;
        var last = 1_000_000_020;

        var numberWithMostDivisors = first;
        var result = 0;

        var watch = Stopwatch.StartNew();
        for (var i = first; i <= last; i++)
        {
            var innerWatch = Stopwatch.StartNew();
            var divisorCounter = await cacheApiClient.GetDivisorCount(i) ?? 0;

            if (divisorCounter == 0)
            {
                for (var divisor = 1; divisor <= i; divisor++)
                {
                    if (i % divisor == 0)
                    {
                        divisorCounter++;
                    }
                }
                await cacheApiClient.SetDivisorCount(i, divisorCounter);
            }

            innerWatch.Stop();
            Console.WriteLine("Counted " + divisorCounter + " divisors for " + i + " in " + innerWatch.ElapsedMilliseconds + "ms");

            if (divisorCounter > result)
            {
                numberWithMostDivisors = i;
                result = divisorCounter;
            }
        }
        watch.Stop();

        Console.WriteLine("The number with most divisors inside range is: " + numberWithMostDivisors + " with " + result + " divisors.");
        Console.WriteLine("Elapsed time: " + watch.ElapsedMilliseconds + "ms");
        Console.ReadLine();
    }
}
Here is the Cache service connection implementation with the port that should match one in docker-compose.yml

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


Here is the CacheController implementation

using CacheService.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace CacheService.Controllers;

[ApiController]
[Route("[controller]")]
public class CacheController : ControllerBase
{
private readonly DatabaseService _databaseService = new DatabaseService();

    [HttpGet]
    public ActionResult<int> Get(long number)
    {
        var result = _databaseService.GetDivisorCount(number);
        if (result == null)
            return NotFound();
        return Ok(result.Value);
    }

    [HttpPost]
    public IActionResult Post([FromQuery] long number, [FromQuery] int divisorCounter)
    {
        _databaseService.SetDivisorCount(number, divisorCounter);
        return Ok();
    }
}


And finally the db logic from DatabaseService

using System.Data;
using Dapper;
using MySqlConnector;

namespace CacheService.DataAccess;

public class DatabaseService
{
private IDbConnection _divisorCache =
new MySqlConnection("Server=cache-db;Database=cache-database;Uid=div-cache;Pwd=C@ch3d1v;");

    public int? GetDivisorCount(long number)
    {
        return _divisorCache.QueryFirstOrDefault<int?>("SELECT divisors FROM counters WHERE number = @number", new { number });
    }

    public void SetDivisorCount(long number, int divisors)
    {
        _divisorCache.Execute("INSERT INTO counters (number, divisors) VALUES (@number, @divisors)", new { number, divisors });
    }
}



And here is updated docker-compose.yml

version: "3.9"

services:
counter-service:
build:
context: ./DivisorCounter
dockerfile: Dockerfile
ports:
- "8000:80"
depends_on:
- cache-db
restart: on-failure

cache-service:
build:
context: ./CacheService
dockerfile: Dockerfile
ports:
- "8001:80"
depends_on:
- cache-db
restart: on-failure

cache-db:
image: "mysql:latest"
environment:
MYSQL_DATABASE: "cache-database"
MYSQL_USER: "div-cache"
MYSQL_PASSWORD: "C@ch3d1v"
MYSQL_RANDOM_ROOT_PASSWORD: "yes"
ports:
- "3306:3306"
restart: on-failure


