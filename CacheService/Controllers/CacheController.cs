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