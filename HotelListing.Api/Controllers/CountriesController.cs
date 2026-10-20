using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Data;
using HotelListing.Api.DTOs.Country;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(HotelListingDBContext context) : ControllerBase
{

    // GET: api/Country
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountry()
    {
        var countries = await context.Countries
            //projection to DTO
            .Select(c => new GetCountriesDto(
                c.CountryId,
                c.Name,
                c.ShortName))
            .ToListAsync();
        //process countries
        return countries;
    }

    // GET: api/Country/5
    [HttpGet("{countryid}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int countryid)
    {
        var country = await context.Countries
            .Where(c => c.CountryId==countryid)
            //.Include(c => c.Hotels)
            .Select(c => new GetCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h => new HotelListing.Api.DTOs.Hotel.GetSlimHotelsDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (country == null)
        {
            return NotFound();
        }

        return country;
    }

    // PUT: api/Country/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{countryid}")]
    public async Task<IActionResult> PutCountry(int countryid, UpdateCountryDto updateDto)
    {
        if (countryid != updateDto.Id)
        {
            return BadRequest();
        }

        var country = await context.Countries.FindAsync(countryid);
        if (country == null)
        {
            return NotFound();
        }

        country.Name = updateDto.Name;
        country.ShortName = updateDto.ShortName;

        context.Entry(country).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExistsAsync(countryid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Country
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            Name = countryDto.Name,
            ShortName = countryDto.ShortName
        };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var resultDto = new GetCountryDto(
            country.CountryId,
            country.Name,
            country.ShortName,
            //null
            []
            );

        return CreatedAtAction(nameof(GetCountry), new { countryid = country.CountryId }, resultDto);
    }

    // DELETE: api/Country/5
    [HttpDelete("{countryid}")]
    public async Task<IActionResult> DeleteCountry(int? countryid)
    {
        var country = await context.Countries.FindAsync(countryid);
        if (country == null)
        {
            return NotFound();
        }

        context.Countries.Remove(country);
        await context.SaveChangesAsync();

        return NoContent();
    }

    
    private async Task<bool> CountryExistsAsync(int? countryid)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == countryid);
    }
}
