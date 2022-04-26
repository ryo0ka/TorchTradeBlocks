using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public readonly struct PanelParam
    {
        public PanelParam(StoreItemTypes itemType, PanelType panelType)
        {
            ItemType = itemType;
            PanelType = panelType;
        }

        public StoreItemTypes ItemType { get; }
        public PanelType PanelType { get; }

        public override string ToString()
        {
            return $"{nameof(ItemType)}: {ItemType}, {nameof(PanelType)}: {PanelType}";
        }
    }
}