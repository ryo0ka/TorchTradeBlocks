using System;
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

            ReloadConfigs();

            _core = new Core.TradeBlocksCore();
        }

        public void ReloadConfigs()
        {
            var configPath = this.MakeConfigFilePath();
            _config = Persistent<Config>.Load(configPath);
            Config.Instance = _config.Data;
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