using System;
using ProtoBuf;
using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    [ProtoContract]
    public sealed class StoreItem
    {
        [ProtoMember(1)]
        public int ServerID { get; set; }

        [ProtoMember(2)]
        public StoreItemTypes Type { get; set; }

        [ProtoMember(3, IsRequired = false)]
        public string Faction { get; set; }

        [ProtoMember(4)]
        public string Player { get; set; }

        [ProtoMember(5)]
        public string Region { get; set; }

        [ProtoMember(6)]
        public string Item { get; set; }

        [ProtoMember(7)]
        public long Amount { get; set; }

        [ProtoMember(8)]
        public int PricePerUnit { get; set; }

        bool Equals(StoreItem other)
        {
            return ServerID == other.ServerID &&
                   Type == other.Type &&
                   Faction == other.Faction &&
                   Player == other.Player &&
                   Region == other.Region &&
                   Item == other.Item &&
                   Amount == other.Amount &&
                   PricePerUnit == other.PricePerUnit;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is StoreItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ServerID;
                hashCode = (hashCode * 397) ^ (int)Type;
                hashCode = (hashCode * 397) ^ (Faction != null ? Faction.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Player != null ? Player.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Region != null ? Region.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Item != null ? Item.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ PricePerUnit;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(ServerID)}: {ServerID}, {nameof(Type)}: {Type}, {nameof(Faction)}: {Faction}, {nameof(Player)}: {Player}, {nameof(Region)}: {Region}, {nameof(Item)}: {Item}, {nameof(Amount)}: {Amount}, {nameof(PricePerUnit)}: {PricePerUnit}";
        }
    }
}