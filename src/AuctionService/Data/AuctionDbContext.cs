namespace AuctionService.Data
{
    using AuctionService.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Auction> Auctions { get; set; }
    }
}