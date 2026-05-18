using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using Microsoft.Extensions.Caching.Memory;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ReportController> _logger;

    public ReportController(AppDbContext db, IMemoryCache cache, ILogger<ReportController> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("complete")]
    public async Task<IActionResult> GetCompleteReport([FromQuery] int? godina, [FromQuery] int? mjesec)
    {
        var g = godina ?? DateTime.Now.Year;
        var m = mjesec ?? DateTime.Now.Month;
        
        _logger.LogInformation("=== Report request received: Year={Year}, Month={Month} ===", g, m);
        
        try
        {
            var cacheKey = $"complete_report_{g}_{m}";
            
            if (_cache.TryGetValue(cacheKey, out object? cachedData))
            {
                _logger.LogInformation("*** CACHE HIT for {CacheKey} ***", cacheKey);
                return Ok(cachedData);
            }
            
            _logger.LogInformation("Cache miss for {CacheKey}, fetching from database...", cacheKey);

            var start = new DateTime(g, m, 1).AddMonths(-11);
            var end = new DateTime(g, m, 1).AddMonths(1);

        // Fetch all data sequentially to avoid DbContext threading issues
        var tvData = await _db.IvrCdrs
            .AsNoTracking()
            .Where(r => r.ReceivedCalledNumber == "125"
                     && r.CallStart.HasValue
                     && r.CallStart >= start
                     && r.CallStart < end)
            .GroupBy(r => new { Godina = r.CallStart!.Value.Year, Mjesec = r.CallStart!.Value.Month })
            .Select(g => new { Godina = g.Key.Godina, Mjesec = g.Key.Mjesec, Broj_Poziva = g.Count() })
            .OrderBy(x => x.Godina).ThenBy(x => x.Mjesec)
            .ToListAsync();

        var prmPrefixes = new[] { "0800", "0902", "0922", "0942" };
        var prmData = await _db.IvrCdrs
            .AsNoTracking()
            .Where(r => r.ReceivedCalledNumber != null
                     && r.ReceivedCalledNumber.Length >= 4
                     && prmPrefixes.Any(p => r.ReceivedCalledNumber.StartsWith(p))
                     && r.CallStart.HasValue
                     && r.CallStart >= start
                     && r.CallStart < end
                     && r.IsIncomingCall == "Y")
            .GroupBy(r => new
            {
                Godina = r.CallStart!.Value.Year,
                Mjesec = r.CallStart!.Value.Month,
                Prefix = r.ReceivedCalledNumber!.Substring(0, 4)
            })
            .Select(g => new
            {
                Godina = g.Key.Godina,
                Mjesec = g.Key.Mjesec,
                Prefix = g.Key.Prefix,
                Trajanje = g.Sum(x => x.TotalDuration - x.EstablishedDuration)
            })
            .OrderBy(x => x.Godina).ThenBy(x => x.Mjesec)
            .ToListAsync();

        var tvPrefixes = new[] { "09622", "09623", "09624", "09625", "09626", "09627" };
        var televotingData = await _db.IvrCdrs
            .AsNoTracking()
            .Where(r => r.ReceivedCalledNumber != null
                     && r.ReceivedCalledNumber.Length >= 5
                     && tvPrefixes.Any(p => r.ReceivedCalledNumber.StartsWith(p))
                     && r.CallStart.HasValue
                     && r.CallStart >= start
                     && r.CallStart < end)
            .GroupBy(r => new
            {
                Godina = r.CallStart!.Value.Year,
                Mjesec = r.CallStart!.Value.Month,
                Prefix = r.ReceivedCalledNumber!.Substring(0, 5)
            })
            .Select(g => new
            {
                Godina = g.Key.Godina,
                Mjesec = g.Key.Mjesec,
                Prefix = g.Key.Prefix,
                Broj_Poziva = g.Count()
            })
            .OrderBy(x => x.Godina).ThenBy(x => x.Mjesec)
            .ToListAsync();

        var kpzDoprinosData = await _db.KpzCdrsZaDan
            .Where(k => k.Datum >= start && k.Datum < end)
            .GroupBy(k => new { k.BrojZatvora, Mjesec = k.Datum.Month, Godina = k.Datum.Year })
            .Select(g => new
            {
                g.Key.BrojZatvora,
                g.Key.Mjesec,
                g.Key.Godina,
                BrojSekundiFiksna = g.Sum(x => x.BrojSekundiFiksna),
                BrojSekundiMobilna = g.Sum(x => x.BrojSekundiMobilna),
                BrojSekundiInternacionalna = g.Sum(x => x.BrojSekundiInternacionalna)
            })
            .OrderBy(o => o.BrojZatvora).ThenBy(o => o.Godina).ThenBy(o => o.Mjesec)
            .ToListAsync();

        var prisons = await _db.Kpzs.OrderBy(k => k.BrojZatvora).ToListAsync();

        var kpzPoziviData = await _db.KpzCdrsZaDan
            .Where(k => k.Datum >= start && k.Datum < end)
            .GroupBy(k => new { k.BrojZatvora, Mjesec = k.Datum.Month, Godina = k.Datum.Year })
            .Select(g => new
            {
                g.Key.BrojZatvora,
                g.Key.Mjesec,
                g.Key.Godina,
                BrojPoziva = g.Sum(x => x.BrojPoziva),
                BrojJavljanja = g.Sum(x => x.BrojJavljanja),
                TrajanjeRazgovora = (g.Sum(x => x.BrojSekundiFiksna) + g.Sum(x => x.BrojSekundiMobilna) + g.Sum(x => x.BrojSekundiInternacionalna)) / 60,
                TrajanjePremaFiksnoj = g.Sum(x => x.BrojSekundiFiksna) / 60,
                TrajanjePremaMobilnoj = g.Sum(x => x.BrojSekundiMobilna) / 60,
                TrajanjePremaInternacionalnoj = g.Sum(x => x.BrojSekundiInternacionalna) / 60
            })
            .OrderBy(o => o.BrojZatvora).ThenBy(o => o.Godina).ThenBy(o => o.Mjesec)
            .ToListAsync();

        // Calculate doprinos
        var kpzDoprinos = kpzDoprinosData.Select(g => new
        {
            g.BrojZatvora,
            g.Mjesec,
            g.Godina,
            Doprinos = ((g.BrojSekundiFiksna / 60) * IzracunajCijenu(g.BrojZatvora, "Fiksna")) +
                ((g.BrojSekundiMobilna / 60) * IzracunajCijenu(g.BrojZatvora, "Mobilna")) +
                ((g.BrojSekundiInternacionalna / 60) * IzracunajCijenu(g.BrojZatvora, "Internacionalna"))
        }).ToList();

        // Group KPZ pozivi by prison
        var kpzPoziviByPrison = kpzPoziviData
            .GroupBy(x => x.BrojZatvora)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Godina).ThenBy(x => x.Mjesec).ToList()
            );

        var result = new
        {
            tacnoVrijeme = tvData,
            prmBrojevi = prmData,
            televoting = televotingData,
            kpzDoprinos = kpzDoprinos,
            prisons = prisons,
            kpzPozivi = kpzPoziviByPrison
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        _logger.LogInformation("*** Data cached with key {CacheKey} for 10 minutes ***", cacheKey);
        return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating complete report for year {Year}, month {Month}", g, m);
            return StatusCode(500, new { error = "An error occurred while generating the report. Please try again later." });
        }
    }

    private decimal IzracunajCijenu(int brojZatvora, string tip)
    {
        return brojZatvora switch
        {
            1 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.2m, "Internacionalna" => 0.65m, _ => 0m },
            2 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.17m, "Internacionalna" => 0.70m, _ => 0m },
            3 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.17m, "Internacionalna" => 0.70m, _ => 0m },
            4 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.14m, "Internacionalna" => 0.65m, _ => 0m },
            5 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.14m, "Internacionalna" => 0.65m, _ => 0m },
            6 => tip switch { "Fiksna" => 0.025m, "Mobilna" => 0.105m, "Internacionalna" => 0.53m, _ => 0m },
            8 => tip switch { "Fiksna" => 0.025m, "Mobilna" => 0.07m, "Internacionalna" => 0.53m, _ => 0m },
            9 => tip switch { "Fiksna" => 0.025m, "Mobilna" => 0.07m, "Internacionalna" => 0.53m, _ => 0m },
            10 => tip switch { "Fiksna" => 0.04m, "Mobilna" => 0.12m, "Internacionalna" => 0.65m, _ => 0m },
            11 => tip switch { "Fiksna" => 0.39m, "Mobilna" => 0.20m, "Internacionalna" => 0.65m, _ => 0m },
            12 => tip switch { "Fiksna" => 0.039m, "Mobilna" => 0.17m, "Internacionalna" => 0.70m, _ => 0m },
            _ => 0m
        };
    }
}
