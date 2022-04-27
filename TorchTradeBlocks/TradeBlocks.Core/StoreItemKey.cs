using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public readonly struct StoreItemKey
    {
        public readonly StoreItemTypes Type;
        public readonly long Player;
        public readonly string Region;
        public readonly string Item;
        public readonly int PricePerUnit;

        public StoreItemKey(StoreItemTypes type, long player, string region, string item, int pricePerUnit)
        {
            Type = type;
            Player = player;
            Region = region;
            Item = item;
            PricePerUnit = pricePerUnit;
        }

        public bool Equals(StoreItemKey other)
        {
            return Type == other.Type && Player == other.Player && Region == other.Region && Item == other.Item && PricePerUnit == other.PricePerUnit;
        }

        public override bool Equals(object obj)
        {
            return obj is StoreItemKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ Player.GetHashCode();
                hashCode = (hashCode * 397) ^ (Region != null ? Region.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Item != null ? Item.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PricePerUnit;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Player)}: {Player}, {nameof(Region)}: {Region}, {nameof(Item)}: {Item}, {nameof(PricePerUnit)}: {PricePerUnit}";
        }
    }
}