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