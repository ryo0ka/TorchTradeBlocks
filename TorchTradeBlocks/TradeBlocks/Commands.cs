using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TradeBlocks.Core;
using Utils.General;
using Utils.Torch;
using VRage.Game.ModAPI;

namespace TradeBlocks
{
    [Category("stores")]
    public sealed class Commands : CommandModule
    {
        public Plugin Plugin => (Plugin)Context.Plugin;

        [Command("reload")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfigs()
        {
            Plugin.ReloadConfigs();
            Context.Respond("reloaded configs");
        }

        [Command("items_local")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ShowItems() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var storeItem in TradeBlocksCore.Instance.LocalStoreItems)
            {
                sb.AppendLine(storeItem.ToString());
            }

            RespondDialog(sb.ToString());
        });

        [Command("items")]
        [Permission(MyPromoteLevel.None)]
        public void ShowAllItems() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var storeItem in TradeBlocksCore.Instance.AllStoreItems)
            {
                sb.AppendLine(storeItem.ToString());
            }

            RespondDialog(sb.ToString());
        });

        [Command("stores")]
        [Permission(MyPromoteLevel.Moderator)]
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

            RespondDialog(sb.ToString());
        });

        [Command("panels")]
        [Permission(MyPromoteLevel.Moderator)]
        public void ShowPanels() => this.CatchAndReport(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var panel in TradeBlocksCore.Instance.LocalPanels)
            {
                if (panel.ParamOrNull is { } param)
                {
                    sb.AppendLine($"\"{panel.Grid.DisplayName}\": {param}");
                }
            }

            RespondDialog(sb.ToString());
        });

        void RespondDialog(string message)
        {
            if (Context.Player != null)
            {
                var msg = new DialogMessage(MySession.Static.Name, "!stores items", message);
                ModCommunication.SendMessageTo(msg, Context.Player.SteamUserId);
                return;
            }

            Context.Respond(message);
        }
    }
}