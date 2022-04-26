using System;
using System.Collections.Generic;
using DefaultNamespace;
using HNZ.Utils;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Utils.Torch;
using VRage.Game.ObjectBuilders.Definitions;
using Utils.General;
using VRageMath;

namespace TradeBlocks.Core
{
    public sealed class Core : CubeBlockMappingCollection<MyTextPanel, Panel>.IMapper
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static Core Instance { get; private set; }

        readonly CubeBlockAddRemoveObserver<MyTextPanel> _panelObserver;
        readonly CubeBlockMappingCollection<MyTextPanel, Panel> _panels;
        readonly CubeBlockAddRemoveObserver<MyStoreBlock> _storeObserver;
        readonly SceneEntityCachingSet<MyStoreBlock> _stores;
        readonly Dictionary<StoreItemTypes, List<StoreItem>> _storeItems;

        public Core()
        {
            Instance = this;

            _panelObserver = new CubeBlockAddRemoveObserver<MyTextPanel>();
            _panels = new CubeBlockMappingCollection<MyTextPanel, Panel>(_panelObserver, this);
            _storeObserver = new CubeBlockAddRemoveObserver<MyStoreBlock>();
            _stores = new SceneEntityCachingSet<MyStoreBlock>(_storeObserver);
            _storeItems = new Dictionary<StoreItemTypes, List<StoreItem>>();
        }

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
        }

        Panel CubeBlockMappingCollection<MyTextPanel, Panel>.IMapper.Map(MyTextPanel entity)
        {
            return new Panel(entity);
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
            _storeItems.Clear();
            foreach (var store in _stores)
            {
                Log.Debug($"store: {store.CubeGrid.DisplayName}");

                if (store.OwnerId == 0) continue; // owned by nobody
                if (MySession.Static.Players.IdentityIsNpc(store.OwnerId)) continue; // owned by npc
                if (!MySession.Static.Players.TryGetPlayerById(store.OwnerId, out var player)) continue;
                var faction = MySession.Static.Factions.GetPlayerFaction(store.OwnerId);
                var region = GetRegion(store.CubeGrid.PositionComp.GetPosition());
                foreach (var item in store.PlayerItems)
                {
                    if (item.Item?.SubtypeName is not { } itemStr) continue;

                    var storeItem = new StoreItem
                    {
                        Faction = faction?.Tag,
                        Player = player.DisplayName,
                        Region = region,
                        Item = itemStr,
                        Amount = item.Amount,
                        PricePerUnit = item.PricePerUnit,
                    };

                    _storeItems.Add(item.StoreItemType, storeItem);
                    Log.Debug($"item in store: {storeItem}");
                }
            }
        }

        public IReadOnlyList<StoreItem> GetStoreItems(StoreItemTypes type)
        {
            return _storeItems.GetValueOrDefault(type, new List<StoreItem>());
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