namespace App1.Model.Trade;

public interface ITrade
{
    ITradeItem SoldItem { get; }
    ITradeItem ReceivedItem { get;}
    bool CanTrade(Inventory inventory);
    void Trade(Inventory inventory);
}