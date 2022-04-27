using System;
using Nexus;
using NLog;
using Torch;
using Torch.API;
using Torch.API.Session;
using Utils.Torch;

namespace TradeBlocks
{
    public sealed class Plugin : TorchPluginBase
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        Core.TradeBlocksCore _core;
        bool _passedFirstFrame;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.OnSessionStateChanged(TorchSessionState.Loaded, OnSessionLoaded);
            this.OnSessionStateChanged(TorchSessionState.Unloading, OnSessionUnloading);

            _core = new Core.TradeBlocksCore();
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