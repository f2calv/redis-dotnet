namespace CasCap.Models;

public struct Tick
{
    public Tick(string Symbol, DateTime Date, double Bid, double Offer)
    {
        this.Symbol = Symbol;
        this.Date = Date;
        this.Bid = Bid;
        this.Offer = Offer;
    }

    public string Symbol { get; }
    public DateTime Date { get; }
    public double Bid { get; }
    public double Offer { get; }
}