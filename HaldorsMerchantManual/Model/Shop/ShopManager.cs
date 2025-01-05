#nullable enable
using System.Collections.Generic;

namespace App1.Model.Shop;

public class ShopManager
{
    private readonly Dictionary<ulong, IShop> _shops = new();

    public void RegisterShop(IShop shop)
    {
        _shops.Add(shop.ShopId, shop);
    }

    public void AttemptTrade(Player player, ulong shopId, int tradeId)
    {
        var inventory = player.m_inventory;
        var shop = _shops[shopId];
        if (shop == null)
            return;
        var trade = shop.GetTrade(tradeId);
        if (trade == null)
            return;
        if (!trade.CanTrade(inventory))
        {
            player.Message(MessageHud.MessageType.Center,
                $"You do not have enough {trade.ReceivedItem.Item.m_shared.m_name} " +
                $"to buy {trade.SoldItem.Item.m_shared.m_name}");
            return;
        }

        trade.Trade(inventory);
    }
}