using Slang.WebApi.i18n;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = Strings.SupportedCultures.Select(c => c.ToString()).ToArray();
    
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRequestLocalization();

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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}