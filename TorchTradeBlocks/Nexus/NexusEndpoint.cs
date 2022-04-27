using System;
using Nexus.API;
using Nexus.BoundarySystem;
using NLog;
using Sandbox.ModAPI;
using Torch.API;
using Torch.Managers;
using Torch.API.Managers;

namespace Nexus
{
    public sealed class NexusEndpoint
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public static readonly NexusEndpoint Instance = new();

        NexusAPI _nexusApi;

        public bool IsAvailable { get; private set; }

        public delegate void HandleMessage(byte[] message);

        public event HandleMessage OnMessageReceived;

        public void TryInitialize(ITorchBase torch, ushort modId)
        {
            var pluginId = new Guid("28a12184-0422-43ba-a6e6-2e228611cca5");
            var pluginManager = torch.Managers.GetManager<PluginManager>();
            if (pluginManager.Plugins.TryGetValue(pluginId, out _))
            {
                IsAvailable = true;
                _nexusApi = new NexusAPI(modId);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(_nexusApi.CrossServerModID, OnMessage);
                
                Log.Info("nexus hook initialized");
                return;
            }

            Log.Warn("no nexus");
        }

        public void Close()
        {
            if (IsAvailable)
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(_nexusApi.CrossServerModID, OnMessage);
            }
        }

        void OnMessage(ushort id, byte[] bytes, ulong arg3, bool arg4)
        {
            if (id == _nexusApi.CrossServerModID)
            {
                OnMessageReceived?.Invoke(bytes);
            }
        }

        public void SendMessageToAllServers(byte[] bytes)
        {
            NexusServerSideAPI.SendMessageToAllServers(ref _nexusApi, bytes);
        }

        public int GetThisServerId()
        {
            return RegionHandler.ThisServer.ServerID;
        }
    }
}