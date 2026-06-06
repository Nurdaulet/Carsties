
using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("search-auction-updated", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("search-auction-deleted", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });
    });
});


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
