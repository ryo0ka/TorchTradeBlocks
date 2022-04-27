using System.Collections.Generic;
using System.Text;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI.Ingame;
using Utils.General;
using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public sealed class Panel : IComparer<StoreItem>
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly MyTextPanel _panel;

        public Panel(MyTextPanel panel)
        {
            _panel = panel;
        }

        public MyCubeGrid Grid => _panel.CubeGrid;

        public void Close()
        {
        }

        public void Update()
        {
            Log.Debug("panel update");

            if (_panel.Closed) return;
            if (!TryGetPanelParam(out var param)) return;

            Log.Debug($"params: {param}");

            var srcStoreItems = TradeBlocksCore.Instance.GetStoreItems();
            if (srcStoreItems.Count == 0) return;

            var storeItems = ListPool<StoreItem>.Get();
            foreach (var storeItem in srcStoreItems)
            {
                if (storeItem.Type == param.ItemType)
                {
                    storeItems.Add(storeItem);
                }
            }

            storeItems.Sort(this);
            Log.Debug($"store items: {storeItems.ToStringSeq()}");

            var builder = new StringBuilder();
            builder.AppendLine($"{param.ItemType}:");

            foreach (var storeItem in storeItems)
            {
                var line = $"[{storeItem.Faction ?? "---"}] {storeItem.Player} ({storeItem.Region}): {storeItem.Item} {storeItem.Amount}x {storeItem.PricePerUnit}sc";
                builder.AppendLine(line);
            }

            ListPool<StoreItem>.Release(storeItems);

            ((IMyTextSurface)_panel).WriteText(builder);
        }

        public bool TryGetPanelParam(out PanelParam panelParam)
        {
            return TryParseCustomData(_panel.CustomData, out panelParam);
        }

        int IComparer<StoreItem>.Compare(StoreItem x, StoreItem y)
        {
            if (x == null) return 1;
            if (y == null) return -1;
            if (TryCompare(x.Faction, y.Faction, out var r)) return r;
            if (TryCompare(x.Player, y.Player, out r)) return r;
            if (TryCompare(x.Region, y.Region, out r)) return r;
            if (TryCompare(x.Item, y.Item, out r)) return r;
            if (TryCompare(x.Amount, y.Amount, out r)) return r;
            if (TryCompare(x.PricePerUnit, y.PricePerUnit, out r)) return r;
            return 0;

            static bool TryCompare<T>(T xx, T yy, out int result)
            {
                result = Comparer<T>.Default.Compare(xx, yy);
                return result != 0;
            }
        }

        static bool TryParseCustomData(string customData, out PanelParam param)
        {
            param = default;
            if (string.IsNullOrEmpty(customData)) return false;
            if (!customData.StartsWith("!stores")) return false;

            var command = customData.Split(' ');
            command.TryGetElementAt(1, out var typeStr);
            command.TryGetElementAt(2, out var panelTypeStr);

            var itemType = typeStr switch
            {
                "offer" => StoreItemTypes.Offer,
                "offers" => StoreItemTypes.Offer,
                "order" => StoreItemTypes.Order,
                "orders" => StoreItemTypes.Order,
                _ => StoreItemTypes.Offer,
            };

            var panelType = panelTypeStr switch
            {
                "normal" => PanelType.Normal,
                "long" => PanelType.Long,
                _ => PanelType.Normal,
            };

            param = new PanelParam(itemType, panelType);

            return true;
        }
    }
}