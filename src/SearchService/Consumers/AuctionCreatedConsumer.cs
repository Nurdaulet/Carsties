using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    private readonly DB _db;

    public AuctionCreatedConsumer(IMapper mapper, DB db)
    {
        _mapper = mapper;
        _db = db;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"Received AuctionCreated event for auction with ID: {context.Message.Id}");

        var item = _mapper.Map<Item>(context.Message);

        if(item.Model == "Foo") throw new ArgumentException("Cannot sell cars of model Foo!");
        
        await _db.SaveAsync(item);
    }
}