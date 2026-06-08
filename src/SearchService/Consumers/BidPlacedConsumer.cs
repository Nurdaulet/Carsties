using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
        private readonly DB _db;

    public BidPlacedConsumer(DB db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"Bid placed: {context.Message.Id} for auction {context.Message.AuctionId} with amount {context.Message.Amount}");
        var auction = await _db.Find<Item>().OneAsync(context.Message.AuctionId);

        if(context.Message.BidStatus.Contains("Accepted")
         && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _db.SaveAsync(auction);
        }
    }
}