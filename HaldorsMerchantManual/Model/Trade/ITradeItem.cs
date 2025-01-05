namespace App1.Model.Trade;

public interface ITradeItem
{
    ItemDrop.ItemData Item { get; }
    int Amount { get; }
}