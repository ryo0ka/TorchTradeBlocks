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
        PanelParam _panelParam; // can be null
        bool _passedFirstFrame;
        int _updateCount;

        public Panel(MyTextPanel panel)
        {
            _panel = panel;
            _panel.CustomDataChanged += OnCustomDataChanged;
        }

        public MyCubeGrid Grid => _panel.CubeGrid;
        public PanelParam ParamOrNull => _panelParam;

        public void Close()
        {
            _panel.CustomDataChanged -= OnCustomDataChanged;
        }

        void OnCustomDataChanged(MyTerminalBlock _)
        {
            _panelParam = null;
            if (PanelParam.TryParseCustomData(_panel.CustomData, out var panelParam, out var error))
            {
                _panelParam = panelParam;
                Log.Debug($"custom data changed: {_panelParam}");
            }
            else if (!string.IsNullOrEmpty(error))
            {
                ((IMyTextSurface)_panel).WriteText(error);
            }
        }

        public void Update()
        {
            _updateCount += 1;

            if (_panel.Closed) return;

            if (!_passedFirstFrame)
            {
                _passedFirstFrame = true;
                OnCustomDataChanged(_panel);
            }

            if (_panelParam is not { } param) return;
            Log.Trace("panel update");

            var allStoreItems = TradeBlocksCore.Instance.GetStoreItems();
            var storeItems = ListPool<StoreItem>.Get();
            foreach (var storeItem in allStoreItems)
            {
                if (Accepts(storeItem, param))
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

            var itemTypeStr = param.ItemType switch
            {
                StoreItemTypes.Offer => "Offers (Selling)",
                StoreItemTypes.Order => "Orders (Buying)",
                _ => "<unknown>",
            };

            builder.AppendLine($"{itemTypeStr}:");

            foreach (var storeItem in storeItemsView)
            {
                var line = Config.Instance.StoreItemDisplayFormat
                    .Replace("${faction}", storeItem.Faction ?? "---")
                    .Replace("${player}", storeItem.Player)
                    .Replace("${region}", storeItem.Region)
                    .Replace("${item}", storeItem.Item)
                    .Replace("${price}", storeItem.PricePerUnit.ToString())
                    .Replace("${amount}", storeItem.Amount.ToString());

                builder.AppendLine(line);
            }

            ListPool<StoreItem>.Release(storeItemsView);

            builder.AppendLine();
            builder.AppendLine(Config.Instance.Footer);

            if (param.Debug)
            {
                builder.AppendLine($"debug: {_updateCount}");
            }

            ((IMyTextSurface)_panel).WriteText(builder);
        }

        bool Accepts(StoreItem storeItem, PanelParam param)
        {
            //Log.Info($"{param.ExcludedPlayerSet}, {param.IncludedPlayerSet}, {Config.Instance.ExcludedPlayerSet}");

            if (param.ItemType != storeItem.Type) return false;
            if (param.IncludedPlayerSet.Count > 0 && !param.IncludedPlayerSet.Contains(storeItem.Player)) return false;
            if (param.ExcludedPlayerSet.Count > 0 && param.ExcludedPlayerSet.Contains(storeItem.Player)) return false;
            return true;
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
    }
}