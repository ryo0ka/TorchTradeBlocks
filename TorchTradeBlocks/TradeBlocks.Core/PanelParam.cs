using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public readonly struct PanelParam
    {
        public PanelParam(StoreItemTypes itemType, int maxLineCount)
        {
            ItemType = itemType;
            MaxLineCount = maxLineCount;
        }

        public StoreItemTypes ItemType { get; }
        public int MaxLineCount { get; }

        public override string ToString()
        {
            return $"{nameof(ItemType)}: {ItemType}, {nameof(MaxLineCount)}: {MaxLineCount}";
        }
    }
}