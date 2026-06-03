
using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

try
{
    var db = await DbInitializer.InitDb(builder.Configuration);
    builder.Services.AddSingleton(db);
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing database: {ex.Message}");
    return;
}

var app = builder.Build();



app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.SyncData(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error syncing data from Auction Service: {ex.Message}");
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5));
