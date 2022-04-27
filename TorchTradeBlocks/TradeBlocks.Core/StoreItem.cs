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

        public override string ToString()
        {
            return $"{nameof(ServerID)}: {ServerID}, {nameof(Type)}: {Type}, {nameof(Faction)}: {Faction}, {nameof(Player)}: {Player}, {nameof(Region)}: {Region}, {nameof(Item)}: {Item}, {nameof(Amount)}: {Amount}, {nameof(PricePerUnit)}: {PricePerUnit}";
        }
    }
}