using Torch;
using Torch.API;
using Torch.API.Session;
using Utils.Torch;

namespace TradeBlocks
{
    public sealed class Plugin : TorchPluginBase
    {
        Core.Core _core;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            this.OnSessionStateChanged(TorchSessionState.Loaded, OnSessionLoaded);
            this.OnSessionStateChanged(TorchSessionState.Unloading, OnSessionUnloading);

            _core = new Core.Core();
        }

        void OnSessionLoaded()
        {
        }

        void OnSessionUnloading()
        {
            _core?.Close();
        }

        public override void Update()
        {
            _core.Update();
        }
    }
}