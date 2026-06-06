using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper;
    private readonly DB _db;

    public AuctionDeletedConsumer(IMapper mapper, DB db)
    {
        _mapper = mapper;
        _db = db;
    }

    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"Received AuctionDeleted event for auction with ID: {context.Message.Id}");

        var result = await _db.DeleteAsync<Item>(context.Message.Id);
        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionDeleted), $"Failed to delete item with ID: {context.Message.Id}");
        }
    }
}