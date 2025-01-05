using App1.Model.Trade;

namespace App1.Model.Shop;

public interface IShop : ILoggable
{
    ITrade GetTrade(int tradeId);
    void SetTrade(ITrade trade);
    void RemoveTrade(ulong tradeId);
    ulong ShopId { get; }
    
}