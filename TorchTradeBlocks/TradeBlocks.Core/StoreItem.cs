using System;

namespace TradeBlocks.Core
{
    public sealed class StoreItem
    {
        public string Faction { get; set; } // can be null
        public string Player { get; set; }
        public string Region { get; set; }
        public string Item { get; set; }
        public int Amount { get; set; }
        public int PricePerUnit { get; set; }

        public override string ToString()
        {
            return $"{nameof(Faction)}: {Faction}, {nameof(Player)}: {Player}, {nameof(Region)}: {Region}, {nameof(Item)}: {Item}, {nameof(Amount)}: {Amount}, {nameof(PricePerUnit)}: {PricePerUnit}";
        }
    }
}