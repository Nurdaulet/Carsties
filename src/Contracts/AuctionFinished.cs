namespace Contracts;

public class AuctionFinished
{
    public string AuctionId { get; set; }
    public string Winner { get; set; }
    public string Seller { get; set; }
    public bool ItemSold { get; set; }
    public int? Amount { get; set; }
}