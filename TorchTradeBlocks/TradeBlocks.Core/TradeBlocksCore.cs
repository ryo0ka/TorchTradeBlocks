using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using HNZ.Utils;
using Nexus;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Utils.General;
using Utils.Torch;
using VRageMath;

namespace TradeBlocks.Core
{
    public sealed class TradeBlocksCore
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static TradeBlocksCore Instance { get; private set; }

        readonly CubeBlockAddRemoveObserver<MyTextPanel> _panelObserver;
        readonly CubeBlockMappingCollection<MyTextPanel, Panel> _panels;
        readonly CubeBlockAddRemoveObserver<MyStoreBlock> _storeObserver;
        readonly SceneEntityCachingSet<MyStoreBlock> _stores;
        readonly List<StoreItem> _localStoreItems;
        readonly Dictionary<int, List<StoreItem>> _remoteStoreItems;
        readonly List<StoreItem> _allStoreItems;

        public TradeBlocksCore()
        {
            Instance = this;

            _panelObserver = new CubeBlockAddRemoveObserver<MyTextPanel>();
            _panels = new CubeBlockMappingCollection<MyTextPanel, Panel>(_panelObserver, e => new Panel(e));
            _storeObserver = new CubeBlockAddRemoveObserver<MyStoreBlock>();
            _stores = new SceneEntityCachingSet<MyStoreBlock>(_storeObserver);
            _localStoreItems = new List<StoreItem>();
            _remoteStoreItems = new Dictionary<int, List<StoreItem>>();
            _allStoreItems = new List<StoreItem>();

            NexusEndpoint.Instance.OnMessageReceived += OnMessageReceived;
        }

        public IReadOnlyList<StoreItem> LocalStoreItems => _localStoreItems;
        public IReadOnlyList<StoreItem> AllStoreItems => _allStoreItems;
        public IEnumerable<MyStoreBlock> LocalStoreBlocks => _stores;
        public IEnumerable<Panel> LocalPanels => _panels.GetAll();

        public void Close()
        {
            _panelObserver.Close();

            foreach (var panel in _panels.GetAll())
            {
                panel.Close();
            }

            _panels.Close();

            _storeObserver.Close();
            _stores.Clear();

            NexusEndpoint.Instance.OnMessageReceived -= OnMessageReceived;
        }

        void OnMessageReceived(byte[] message)
        {
            var storeItems = ListPool<StoreItem>.Get();
            DeserializeStoreItemList(message, out var serverId, storeItems);

            Log.Debug("received items from nexus");

            if (!_remoteStoreItems.ContainsKey(serverId))
            {
                _remoteStoreItems[serverId] = storeItems;
                return;
            }

            _remoteStoreItems[serverId].Clear();
            _remoteStoreItems[serverId].AddRange(storeItems);

            Log.Debug($"store items received: {serverId}");
        }

        public void Update()
        {
            if (VRageUtils.EverySeconds(5))
            {
                UpdateEconomy();
            }

            if (VRageUtils.EveryFrame(5))
            {
                foreach (var panel in _panels.Throttle(5))
                {
                    panel.Update();
                }
            }
        }

        void UpdateEconomy()
        {
            Log.Debug("update economy");

            _stores.ApplyChanges();
            _localStoreItems.Clear();
            foreach (var store in _stores)
            {
                Log.Debug($"store: {store.CubeGrid.DisplayName}");

                if (store.OwnerId == 0) continue; // owned by nobody
                if (MySession.Static.Players.IdentityIsNpc(store.OwnerId)) continue; // owned by npc
                var steamId = Sync.Players.TryGetSteamId(store.OwnerId);
                var playerName = MySession.Static.Players.TryGetIdentityNameFromSteamId(steamId); 
                var faction = MySession.Static.Factions.GetPlayerFaction(store.OwnerId);
                var region = GetRegion(store.CubeGrid.PositionComp.GetPosition());
                var serverId = NexusEndpoint.Instance.IsAvailable ? NexusEndpoint.Instance.GetThisServerId() : 0;
                foreach (var item in store.PlayerItems)
                {
                    if (item.Item?.SubtypeName is not { } itemStr) continue;

                    var storeItem = new StoreItem
                    {
                        ServerID = serverId,
                        Type = item.StoreItemType,
                        Faction = faction?.Tag,
                        Player = playerName,
                        Region = region,
                        Item = itemStr,
                        Amount = item.Amount,
                        PricePerUnit = item.PricePerUnit,
                    };

                    _localStoreItems.Add(storeItem);
                    Log.Trace($"item in store: {storeItem}");
                }
                
                Log.Debug($"store done: {store.CubeGrid.DisplayName}");
            }

            // sync stores to all other servers
            if (NexusEndpoint.Instance.IsAvailable)
            {
                var serverId = NexusEndpoint.Instance.GetThisServerId();
                var message = SerializeStoreItemList(serverId, _localStoreItems);
                NexusEndpoint.Instance.SendMessageToAllServers(message);
                Log.Debug("sent store items to nexus");
            }

            _allStoreItems.Clear();
            _allStoreItems.AddRange(_localStoreItems);
            foreach (var (_, remoteStoreItems) in _remoteStoreItems)
            {
                _allStoreItems.AddRange(remoteStoreItems);
            }
        }

        public IReadOnlyList<StoreItem> GetStoreItems()
        {
            return _allStoreItems;
        }

        static string GetRegion(Vector3D position)
        {
            var planet = MyGamePruningStructure.GetClosestPlanet(position);
            if (planet == null) return "Space";

            var gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out _);
            if (gravity.Length() > 0) return planet.Name ?? "noname";

            return "Space";
        }

        static byte[] SerializeStoreItemList(int serverId, IReadOnlyList<StoreItem> storeItems)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(serverId);
            writer.Write(storeItems.Count);
            foreach (var storeItem in storeItems)
            {
                writer.WriteProtobuf(storeItem);
            }

            return stream.ToArray();
        }

        static void DeserializeStoreItemList(byte[] message, out int serverId, ICollection<StoreItem> storeItems)
        {
            using var stream = new MemoryStream(message);
            using var reader = new BinaryReader(stream);

            serverId = reader.ReadInt32();
            var length = reader.ReadInt32();
            for (var i = 0; i < length; i++)
            {
                var storeItem = reader.ReadProtobuf<StoreItem>();
                storeItems.Add(storeItem);
            }
        }
    }
}