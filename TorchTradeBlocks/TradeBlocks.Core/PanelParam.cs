using System.Collections.Generic;
using CommandLine;
using Utils.General;
using VRage.Game.ObjectBuilders.Definitions;

namespace TradeBlocks.Core
{
    public sealed class PanelParam
    {
        const string Head = "!stores";

        [Option('t', "type", Required = true)]
        public StoreItemTypes ItemType { get; set; }

        [Option('l', "lines")]
        public int MaxLineCount { get; set; }

        [Option('i', "included")]
        public IEnumerable<string> IncludedPlayers { get; set; }

        [Option('e', "excluded")]
        public IEnumerable<string> ExcludedPlayers { get; set; }

        public HashSet<string> IncludedPlayerSet { get; } = new();
        public HashSet<string> ExcludedPlayerSet { get; } = new();

        public static bool TryParseCustomData(string customData, out PanelParam param, out string error)
        {
            param = default;
            error = default;
            if (string.IsNullOrEmpty(customData)) return false;
            if (!customData.StartsWith(Head)) return false;

            var argStr = customData.Substring(Head.Length, customData.Length - Head.Length);
            var args = LangUtils.ParseArguments(argStr);
            var r = Parser.Default.ParseArguments<PanelParam>(args);
            if (r is NotParsed<PanelParam> e)
            {
                error = e.Errors.ToStringSeq("\n ");
                return false;
            }

            param = ((Parsed<PanelParam>)r).Value;
            param.IncludedPlayerSet.UnionWith(param.IncludedPlayers);
            param.ExcludedPlayerSet.UnionWith(param.ExcludedPlayers);
            return true;
        }

        public override string ToString()
        {
            return $"{nameof(ItemType)}: {ItemType}, {nameof(MaxLineCount)}: {MaxLineCount}";
        }
    }
}