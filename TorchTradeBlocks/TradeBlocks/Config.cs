using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using NLog;
using Torch;
using Torch.Views;
using Utils.Torch;

namespace TradeBlocks
{
    public sealed class Config : ViewModel, FileLoggingConfigurator.IConfig
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public const string DefaultLogFilePath = "Logs/TradeBlocks-${shortdate}.log";
        public static Config Instance { get; set; }

        [XmlIgnore]
        public readonly HashSet<string> ExcludedPlayerSet = new();

        [XmlIgnore]
        public readonly HashSet<string> ExcludedItemSet = new() { "ClangCola", "CosmicCoffee" };

        string _storeItemDisplayFormat = "[${faction}] ${player} (${region}): ${item} ${price}: ${amount}x";
        bool _suppressWpfOutput;
        bool _enableLoggingTrace;
        bool _enableLoggingDebug;
        string _logFilePath = DefaultLogFilePath;
        string _footer = "This panel reflects every player-made\nstore block in the world";

        [XmlElement, Display]
        public string StoreItemDisplayFormat
        {
            get => _storeItemDisplayFormat;
            set => SetValue(ref _storeItemDisplayFormat, value);
        }

        [XmlElement, Display]
        public string Footer
        {
            get => _footer;
            set => SetValue(ref _footer, value);
        }

        [XmlArray, Display]
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

        [XmlArray, Display]
        public List<string> ExcludedItems
        {
            get => ExcludedItemSet.ToList();
            set
            {
                ExcludedItemSet.Clear();
                ExcludedItemSet.UnionWith(value);
                OnPropertyChanged(nameof(ExcludedItemSet));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
            }
        }

        [XmlElement, Display(GroupName = "Logs")]
        public bool SuppressWpfOutput
        {
            get => _suppressWpfOutput;
            set => SetValue(ref _suppressWpfOutput, value);
        }

        [XmlElement, Display(GroupName = "Logs")]
        public bool EnableLoggingTrace
        {
            get => _enableLoggingTrace;
            set => SetValue(ref _enableLoggingTrace, value);
        }

        [XmlElement, Display(GroupName = "Logs")]
        public bool EnableLoggingDebug
        {
            get => _enableLoggingDebug;
            set => SetValue(ref _enableLoggingDebug, value);
        }

        [XmlElement, Display(GroupName = "Logs")]
        public string LogFilePath
        {
            get => _logFilePath;
            set => SetValue(ref _logFilePath, value);
        }
    }
}