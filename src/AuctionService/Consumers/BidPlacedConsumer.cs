using AuctionService.Data;
using Contracts;
using MassTransit;

namespace  AuctionService;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private AuctionDbContext _dbContext;

    public BidPlacedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine($"Received BidPlaced event for AuctionId: {context.Message.AuctionId}, Bidder: {context.Message.Bidder}, Amount: {context.Message.Amount}");
        
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        if(auction.CurrentHighBid == null || context.Message.BidStatus.Contains("Accepted") && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }
    }
}