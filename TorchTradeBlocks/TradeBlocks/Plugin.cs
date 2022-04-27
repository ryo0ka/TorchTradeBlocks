using System;
using System.ComponentModel;
using System.Windows.Controls;
using Nexus;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.API.Session;
using Utils.Torch;

namespace TradeBlocks
{
    public sealed class Plugin : TorchPluginBase, IWpfPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        Persistent<Config> _config;
        UserControl _userControl;
        FileLoggingConfigurator _loggingConfigurator;
        Core.TradeBlocksCore _core;
        bool _passedFirstFrame;

        UserControl IWpfPlugin.GetControl()
        {
            return _config.GetOrCreateUserControl(ref _userControl);
        }

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.OnSessionStateChanged(TorchSessionState.Loaded, OnSessionLoaded);
            this.OnSessionStateChanged(TorchSessionState.Unloading, OnSessionUnloading);

            _loggingConfigurator = new FileLoggingConfigurator(
                "TradeBlocks",
                new[] { $"{nameof(TradeBlocks)}.*" },
                Config.DefaultLogFilePath);
            _loggingConfigurator.Initialize();

            ReloadConfigs();

            _core = new Core.TradeBlocksCore();
        }

        public void ReloadConfigs()
        {
            if (Config.Instance != null)
            {
                Config.Instance.PropertyChanged -= OnConfigChanged;
            }

            var configPath = this.MakeConfigFilePath();
            _config = Persistent<Config>.Load(configPath);
            Config.Instance = _config.Data;
            Config.Instance.PropertyChanged += OnConfigChanged;

            _loggingConfigurator.Configure(Config.Instance);
        }

        void OnConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            _loggingConfigurator.Configure(Config.Instance);
        }

        void OnSessionLoaded()
        {
        }

        public override void Update()
        {
            if (!_passedFirstFrame)
            {
                _passedFirstFrame = true;
                var modId = (ushort)nameof(TorchTradeBlocks).GetHashCode();
                NexusEndpoint.Instance.TryInitialize(Torch, modId);
            }

            _core.Update();
        }

        void OnSessionUnloading()
        {
            _core?.Close();
            NexusEndpoint.Instance.Close();
        }
    }
}