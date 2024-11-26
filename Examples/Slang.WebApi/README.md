# Slang.Net - Web Api Example

## Localization Configuration

Set up localization by configuring supported cultures and default culture.

### Configuration Code

```csharp
using Slang.WebApi.i18n;

var builder = WebApplication.CreateBuilder(args);

// Configure supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = Strings.SupportedCultures.Select(c => c.ToString()).ToArray();

    options.SetDefaultCulture(supportedCultures[0]) 
        .AddSupportedCultures(supportedCultures)    
        .AddSupportedUICultures(supportedCultures); 
});
```

## Middleware for Localization

Add the UseRequestLocalization middleware to the request processing pipeline:

```csharp
app.UseRequestLocalization();
```

## Localization in Controllers

Use the Loc property to access the localized strings:

```csharp
[Route("api/[controller]")]
[ApiController]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        var texts = Strings.Loc.Weather;

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: Random.Shared.Next(-20, 55),
                    Summary: texts.Summaries[Random.Shared.Next(texts.Summaries.Count)]
                ))
            .ToArray();
        
        return forecast;
    }
}
```

or Minimal API

```csharp
app.MapGet("/weatherforecast", () =>
    {
        var texts = Strings.Loc.Weather;

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    Date: DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC: Random.Shared.Next(-20, 55),
                    Summary: texts.Summaries[Random.Shared.Next(texts.Summaries.Count)]
                ))
            .ToArray();
        
        return forecast;
    })
    .WithName("GetWeatherForecast");
```

## Testing the Application

To test the application, you can use the following command:

```bash
curl -H "Accept-Language: en-US" "https://localhost:7133/weatherforecast"
```

This will return a JSON response with the localized strings.