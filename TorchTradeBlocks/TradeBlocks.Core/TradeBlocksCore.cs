using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using HNZ.Utils;
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
        readonly SceneEntityCachingSet<MyStoreBlock> _allStores;
        readonly List<StoreItem> _allStoreItems;

        public TradeBlocksCore()
        {
            Instance = this;

            _panelObserver = new CubeBlockAddRemoveObserver<MyTextPanel>();
            _panels = new CubeBlockMappingCollection<MyTextPanel, Panel>(_panelObserver, e => new Panel(e));
            _storeObserver = new CubeBlockAddRemoveObserver<MyStoreBlock>();
            _allStores = new SceneEntityCachingSet<MyStoreBlock>(_storeObserver);
            _allStoreItems = new List<StoreItem>();
        }

        public IReadOnlyList<StoreItem> AllStoreItems => _allStoreItems;
        public IEnumerable<MyStoreBlock> AllStoreBlocks => _allStores;
        public IEnumerable<Panel> AllPanels => _panels.GetAll();

        public void Close()
        {
            _panelObserver.Close();

            foreach (var panel in _panels.GetAll())
            {
                panel.Close();
            }

            _panels.Close();

            _storeObserver.Close();
            _allStores.Clear();
        }

        public void Update()
        {
            if (VRageUtils.EverySeconds(5))
            {
                UpdateEconomy();
            }

            const float minUpdateFrequency = 5f;
            var count = _panels.Count / (minUpdateFrequency * 60);
            var frame = count >= 1 ? 1 : (int)(1 / count);
            if (VRageUtils.EveryFrame(frame))
            {
                var c = count >= 1 ? (int)count : 1;
                foreach (var panel in _panels.Throttle(c))
                {
                    try
                    {
                        panel.Update();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        void UpdateEconomy()
        {
            Log.Debug("update economy");

            // collect local store items
            _allStoreItems.Clear();
            _allStores.ApplyChanges();
            var storeItems = DictionaryPool<StoreItemKey, long>.Get();
            foreach (var store in _allStores)
            {
                Log.Debug($"store: {store.CubeGrid.DisplayName}");

                if (store.OwnerId == 0) continue; // owned by nobody
                if (MySession.Static.Players.IdentityIsNpc(store.OwnerId)) continue; // owned by npc
                var region = GetRegion(store.CubeGrid.PositionComp.GetPosition());
                foreach (var item in store.PlayerItems)
                {
                    if (item.Item?.SubtypeName is not { } itemStr) continue;

                    var key = new StoreItemKey(item.StoreItemType, store.OwnerId, region, itemStr, item.PricePerUnit);
                    var amount = item.Amount;
                    storeItems.TryGetValue(key, out var amountSum);
                    storeItems[key] = amountSum + amount;

                    Log.Trace($"item in store: {key}, amount: {amount}");
                }

                Log.Debug($"store done: {store.CubeGrid.DisplayName}");
            }

            foreach (var (key, amount) in storeItems)
            {
                var steamId = Sync.Players.TryGetSteamId(key.Player);
                var playerName = MySession.Static.Players.TryGetIdentityNameFromSteamId(steamId);
                var faction = MySession.Static.Factions.GetPlayerFaction(key.Player);
                var storeItem = new StoreItem
                {
                    Type = key.Type,
                    Faction = faction?.Tag,
                    Player = playerName,
                    Region = key.Region,
                    Item = key.Item,
                    Amount = amount,
                    PricePerUnit = key.PricePerUnit,
                };

                if (Accepts(storeItem))
                {
                    _allStoreItems.Add(storeItem);
                }
            }

            DictionaryPool<StoreItemKey, long>.Release(storeItems);
        }

        bool Accepts(StoreItem storeItem)
        {
            if (Config.Instance.ExcludedPlayerSet.Contains(storeItem.Player)) return false;
            if (Config.Instance.ExcludedItemSet.Contains(storeItem.Item)) return false;
            return true;
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
    }
}