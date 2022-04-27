using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using Torch;
using Torch.Views;

namespace TradeBlocks
{
    public sealed class Config : ViewModel
    {
        public static Config Instance { get; set; }

        public readonly HashSet<string> ExcludedPlayerSet = new();

        [XmlElement, Display]
        public List<string> ExcludedPlayers
        {
            get => ExcludedPlayerSet.ToList();
            set
            {
                ExcludedPlayerSet.Clear();
                ExcludedPlayerSet.UnionWith(value);
                OnPropertyChanged(nameof(ExcludedPlayers));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
            }
        }
    }
}