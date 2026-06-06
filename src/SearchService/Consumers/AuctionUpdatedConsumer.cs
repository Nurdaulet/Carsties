using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    private readonly DB _db;

    public AuctionUpdatedConsumer(IMapper mapper, DB db)
    {
        _mapper = mapper;
        _db = db;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"Received AuctionUpdated event for auction with ID: {context.Message.Id}");

        var item = _mapper.Map<Item>(context.Message);

        var result = await _db.Update<Item>()
            .Match(i => i.ID == context.Message.Id)
            .ModifyOnly(i => new
            {
                i.Color,
                i.Make,
                i.Model,
                i.Year,
                i.Mileage
            }, item)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), $"Failed to update item with ID: {context.Message.Id}");
        }
    }
}