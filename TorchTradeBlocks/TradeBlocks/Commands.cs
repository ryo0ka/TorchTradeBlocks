using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities;
using Torch.Commands;
using TradeBlocks.Core;
using Utils.General;
using Utils.Torch;

namespace TradeBlocks
{
    [Category("trades")]
    public sealed class Commands : CommandModule
    {
        [Command("items")]
        public void ShowItems() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var storeItem in TradeBlocksCore.Instance.LocalStoreItems)
            {
                sb.AppendLine(storeItem.ToString());
            }

            Context.Respond(sb.ToString());
        });

        [Command("items_all")]
        public void ShowAllItems() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var storeItem in TradeBlocksCore.Instance.AllStoreItems)
            {
                sb.AppendLine(storeItem.ToString());
            }

            Context.Respond(sb.ToString());
        });

        [Command("stores")]
        public void ShowStoreBlocks() => this.CatchAndReport(() =>
        {
            var counts = new Dictionary<MyCubeGrid, int>();
            foreach (var storeBlock in TradeBlocksCore.Instance.LocalStoreBlocks)
            {
                counts.Increment(storeBlock.CubeGrid);
            }

            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var (grid, storeBlockCount) in counts)
            {
                sb.AppendLine($"\"{grid.DisplayName}\": {storeBlockCount}x");
            }

            Context.Respond(sb.ToString());
        });

        [Command("panels")]
        public void ShowPanels() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var panel in TradeBlocksCore.Instance.LocalPanels)
            {
                if (panel.TryGetPanelParam(out var param))
                {
                    sb.AppendLine($"\"{panel.Grid.DisplayName}\": {param}");
                }
            }

            Context.Respond(sb.ToString());
        });
    }
}