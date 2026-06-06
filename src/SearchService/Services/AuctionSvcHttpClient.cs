using MongoDB.Entities;

namespace SearchService;


public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly DB _db;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration configuration, DB db)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _db = db;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await _db.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();
        
        return await _httpClient.GetFromJsonAsync<List<Item>>($"{_configuration["AuctionSvcUrl"]
        }/api/auctions?date={lastUpdated}");
    }
    
}