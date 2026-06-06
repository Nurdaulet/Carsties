
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public static class DbInitializer
{
    public static async Task<DB> InitDb(IConfiguration config)
    {
        var db = await DB.InitAsync(
            "SearchDb",
            MongoClientSettings.FromConnectionString(
                config.GetConnectionString("MongoDbConnection")));

        await db.Index<Item>()
            .Key(i => i.Make, KeyType.Text)
            .Key(i => i.Model, KeyType.Text)
            .Key(i => i.Color, KeyType.Text)
            .CreateAsync();

        return db;
    }

    public static async Task SyncData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();

        Console.WriteLine(items.Count + " items retrieved from Auction Service.");
        if (items.Count > 0)
        {
            var db = scope.ServiceProvider.GetRequiredService<DB>();
            await db.SaveAsync(items);
        }
    }
}
