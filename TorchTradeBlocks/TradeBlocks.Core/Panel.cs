using System.Collections.Generic;
using System.Text;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.ModAPI.Ingame;
using Utils.General;
using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public sealed class Panel : IComparer<StoreItem>
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        readonly MyTextPanel _panel;
        PanelParam? _panelParam;
        bool _passedFirstFrame;

        public Panel(MyTextPanel panel)
        {
            _panel = panel;
            _panel.CustomDataChanged += OnCustomDataChanged;
        }

        public MyCubeGrid Grid => _panel.CubeGrid;
        public PanelParam? PanelParam => _panelParam;

        public void Close()
        {
            _panel.CustomDataChanged -= OnCustomDataChanged;
        }

        void OnCustomDataChanged(MyTerminalBlock _)
        {
            _panelParam = null;
            if (TryParseCustomData(_panel.CustomData, out var panelParam))
            {
                _panelParam = panelParam;
                Log.Debug($"custom data changed: {_panelParam}");
            }
        }

        public void Update()
        {
            if (_panel.Closed) return;

            if (!_passedFirstFrame)
            {
                _passedFirstFrame = true;
                OnCustomDataChanged(_panel);
            }

            if (_panelParam is not { } param) return;
            Log.Trace("panel update");

            var allStoreItems = TradeBlocksCore.Instance.GetStoreItems();
            if (allStoreItems.Count == 0) return;

            var storeItems = ListPool<StoreItem>.Get();
            foreach (var storeItem in allStoreItems)
            {
                if (storeItem.Type == param.ItemType)
                {
                    storeItems.Add(storeItem);
                }
            }

            storeItems.Sort(this);
            Log.Trace($"store items: {storeItems.ToStringSeq()}");

            var storeItemsView = ListPool<StoreItem>.Get();
            View(storeItems, storeItemsView, param.MaxLineCount);

            ListPool<StoreItem>.Release(storeItems);

            var builder = new StringBuilder();
            builder.AppendLine($"{param.ItemType}:");

            foreach (var storeItem in storeItemsView)
            {
                var line = $"[{storeItem.Faction ?? "---"}] {storeItem.Player} ({storeItem.Region}): {storeItem.Item} {storeItem.Amount}x {storeItem.PricePerUnit}sc";
                builder.AppendLine(line);
            }

            ListPool<StoreItem>.Release(storeItemsView);

            ((IMyTextSurface)_panel).WriteText(builder);
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

        static void View(IReadOnlyList<StoreItem> src, List<StoreItem> dst, int maxLineCount)
        {
            if (maxLineCount == 0)
            {
                dst.AddRange(src);
                return;
            }

            if (src.Count <= maxLineCount)
            {
                dst.AddRange(src);
                return;
            }

            var interval = MySession.Static.GameplayFrameCounter / 60;
            //interval = src.Count - (interval % src.Count);
            for (var i = 0; i < maxLineCount; i++)
            {
                var j = (i + interval) % src.Count;
                dst.Add(src[j]);
            }
        }

        static bool TryParseCustomData(string customData, out PanelParam param)
        {
            param = default;
            if (string.IsNullOrEmpty(customData)) return false;
            if (!customData.StartsWith("!stores")) return false;

            var command = customData.Split(' ');
            command.TryGetElementAt(1, out var typeStr);
            command.TryGetElementAt(2, out var maxLineCountStr);

            var itemType = typeStr switch
            {
                "offer" => StoreItemTypes.Offer,
                "offers" => StoreItemTypes.Offer,
                "order" => StoreItemTypes.Order,
                "orders" => StoreItemTypes.Order,
                _ => StoreItemTypes.Offer,
            };

            int.TryParse(maxLineCountStr ?? "0", out var maxLineCount);

            param = new PanelParam(itemType, maxLineCount);

            return true;
        }
    }
}