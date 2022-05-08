using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game;
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

        [Command("configs")]
        [Permission(MyPromoteLevel.None)]
        public void ShowConfigs()
        {
            this.GetOrSetProperty(Config.Instance);
        }

        [Command("commands")]
        [Permission(MyPromoteLevel.None)]
        public void ShowCommands()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var (Command, Permission) in CommandModuleUtils.GetCommandMethods(typeof(Commands)))
            {
                sb.AppendLine($"!stores {Command.Name} -- [{Permission}] {Command.Description}");
            }

            RespondDialog(sb.ToString());
        }

        [Command("help")]
        [Permission(MyPromoteLevel.None)]
        public void Help()
        {
            this.ShowUrl("https://github.com/HnZGaming/Gaalsien/wiki/Economy");
        }

        [Command("reload")]
        [Permission(MyPromoteLevel.Admin)]
        public void ReloadConfigs()
        {
            Plugin.ReloadConfigs();
            Context.Respond("reloaded configs");
        }

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
            foreach (var storeBlock in TradeBlocksCore.Instance.AllStoreBlocks)
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
            foreach (var panel in TradeBlocksCore.Instance.AllPanels)
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