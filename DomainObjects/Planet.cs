using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Game
{
	public class Planet : Character, IStation, IInteractive
	{
		public event System.Action OnInventoryChanged;

		[Networked] public NetworkObjectGuid PlanetId { get; set; }

		[Networked(OnChanged = nameof(OnMarketInventoryItemsChanged), OnChangedTargets = OnChangedTargets.All), Capacity(64)]
		public NetworkArray<MarketInventoryItemStruct> MarketInventoryItemStructs { get; }

		[Networked(OnChanged = nameof(OnPlanetUpgradesChanged), OnChangedTargets = OnChangedTargets.All), Capacity(16)]
		public NetworkDictionary<int, int> PlanetUpgrades { get; }

		[Networked(OnChanged = nameof(OnTransportStopActionsChanged), OnChangedTargets = OnChangedTargets.All), Capacity(32)]
		public NetworkArray<TransportStopActionStruct> TransportStopActionStructs { get; }

		public PlanetDefinition Def => BaseDef as PlanetDefinition;
		public List<MarketInventoryItem> MarketInventoryItems { get; set; } = new List<MarketInventoryItem>();
		public Dictionary<int, Upgrade> Upgrades = new Dictionary<int, Upgrade>();
		public Dictionary<int, Upgrade> GetUpgrades()
		{
			return Upgrades;
		}
		public Upgrade GetUpgrade(int upgradeDefId)
		{
			return Upgrades[upgradeDefId];
		}

		public LightSource LightSource { get; private set; } = null;
		bool _isLightSourceSet;

		bool _isQuitting;

		override protected void Awake()
		{
			base.Awake();

			LightSource = GetComponent<LightSource>();

			_poi = Instantiate(PlanetManager.Instance.PlanetCompassPOIPrefab, transform);
			_poi.title = Def.Title;

			if (GameManager.IsServer)
				gameObject.AddComponent<RotateSelf>().Speed = Random.Range(.005f, .015f);
		}

		void OnApplicationQuit() {
			_isQuitting = true;
		}

		void OnDestroy() {
			if (!_isQuitting)
				PlanetManager.RemovePlanet(this);
		}

		void Start()
		{
			PlanetManager.AddPlanet(this);
			InvokeRepeating(nameof(InitLightSource), .15f, .15f);
			gameObject.AddComponent<PlanetVisualSimulation>();
		}

		public override void Spawned()
		{
			base.Spawned();

			PlanetId = System.Guid.NewGuid();

			var upgradeCostMaps = Def.ClassDef.PlanetUpgradeCostMaps;
			foreach(var costMap in upgradeCostMaps.OrderBy(x => x.PlanetUpgradeDef.Id))
			{
				Upgrades[costMap.PlanetUpgradeDef.Id] = System.Activator.CreateInstance(costMap.PlanetUpgradeDef.Script, new object[] { this, costMap.PlanetUpgradeDef, 0 }) as Upgrade; //new Upgrade(this, upgradeDef, 0);
			}

			if (!Object.HasStateAuthority)
				return;

			if (_nav)
				_nav.enabled = false;

			foreach(var costMap in upgradeCostMaps.OrderBy(x => x.PlanetUpgradeDef.Id))
			{
				PlanetUpgrades.Add(costMap.PlanetUpgradeDef.Id, 0);
			}

			MarketInventoryItems.Clear();

			int currentIndex = 0;
			for(int i=0; i<Def.ClassDef.ProductionItems.Count; i++) // add production resources and their amounts
			{
				var prod = Def.ClassDef.ProductionItems[i];

				MarketInventoryItem mii = new MarketInventoryItem();
				mii.ItemType = ItemCategory.Resource;
				mii.Amount = Random.Range(prod.StartingAmountMin*10, prod.StartingAmountMax*10);
				mii.Price = 0;
				mii.TransactionType = MarketItemTransactionType.Sell;
				mii.ResourceDef = prod.ResourceDef;

				MarketInventoryItemStruct st = new MarketInventoryItemStruct();
				mii.Fill(ref st);
				MarketInventoryItemStructs.Set(currentIndex++, st);
			}

			foreach(var costMap in upgradeCostMaps.OrderBy(x => x.PlanetUpgradeDef.Id))
			{
				MarketInventoryItem mii = new MarketInventoryItem();
				mii.ItemType = ItemCategory.PlanetUpgrade;
				mii.Amount = 1;
				mii.Price = costMap.PlanetUpgradeDef.GetLevelPrice(PlanetUpgrades[costMap.PlanetUpgradeDef.Id] + 1);
				mii.TransactionType = MarketItemTransactionType.Sell;
				mii.PlanetUpgradeDef = costMap.PlanetUpgradeDef;

				MarketInventoryItemStruct st = new MarketInventoryItemStruct();
				mii.Fill(ref st);
				MarketInventoryItemStructs.Set(currentIndex++, st);
			}

			// add the rest of the resources it does not produce as 0 amounts
			foreach(var res in Defs.GetResourceDefs().OrderBy(x => x.Id))
			{
				MarketInventoryItemStruct st = new MarketInventoryItemStruct();

				bool found = false;
				for(int i=0; i<currentIndex+1; i++)
				{
					st = MarketInventoryItemStructs.Get(i);
					if (st.ResourceDefId == res.Id)
					{
						found = true;
						break;
					}
				}

				if (found)
					continue;

				st.ResourceDefId = res.Id;
				st.Amount = 0;
				st.ItemType = ItemCategory.Resource;
				st.TransactionType = MarketItemTransactionType.Sell;
				MarketInventoryItemStructs.Set(currentIndex++, st);
			}

			Invoke(nameof(OnSpawnedDelayed), 1f);
		}

		void OnSpawnedDelayed() {
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			if (!Object.HasStateAuthority)
				return;

			Upgrades.ForEach(kvp => kvp.Value.FixedUpdateNetwork(Runner, this));
		}

		void InitLightSource()
		{
			Transform src = null;
			if (src == null)
			{
				var stars = GameObject.FindObjectsOfType<Star>();
				if (stars.Length > 0)
				{
					var nearestStar = stars.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault();
					if (nearestStar && Vector3.Distance(nearestStar.transform.position, transform.position) < PlanetManager.Instance.SunLightSourceDistance)
					{
						src = nearestStar.transform;
						CancelInvoke(nameof(InitLightSource));
					}
				}
			}
			SetLightSource(src);

			if (_poi && LocalPlayer.PlayerId == PlayerId && _poi.visibility != CompassNavigatorPro.POI_VISIBILITY.AlwaysVisible)
			{
				_poi.visibility = CompassNavigatorPro.POI_VISIBILITY.AlwaysVisible;
				_poi.tintColor = Color.white;
			}

		}

		void SetLightSource(Transform src)
		{
			_isLightSourceSet = src != null;
			LightSource.Sun = src?.gameObject;
		}

		public void NearbyUpdate(ICharacter entity)
		{
			if (LocalPlayer.Ship == entity)
			{
				var showInfo = Vector3.Distance(LocalPlayer.Ship.transform.position, transform.position) <= InfoDistanceMax;
				if (showInfo)
				{
					if (!UIManager.Instance.PlanetInfoPanel.IsShown)
					{
						UIManager.Instance.PlanetInfoPanel.transform.parent.position = new Vector3(transform.position.x, transform.position.y-40, transform.position.z);
						UIManager.Instance.PlanetInfoPanel.SetPlanet(this);
					}
					UIManager.Instance.PlanetInfoPanel.Show(true, CanInteract(LocalPlayer.Ship));
				}
				else
				{
					if (UIManager.Instance.PlanetInfoPanel.IsShown)
						UIManager.Instance.PlanetInfoPanel.Show(false);
					if (UIManager.Instance.StationPanel.Data == this)
						UIManager.Instance.StationPanel.Hide();
				}
			}
		}

		public override bool CanInteract(ICharacter entity)
		{
			return base.CanInteract(entity) && (PlayerId == entity.PlayerId);
		}

		public void Use(ICharacter entity)
		{
			if (!CanInteract(LocalPlayer.Ship)) return;
			UIManager.TogglePanel("station", this);
		}

        public bool SubtractMarketItem(int index, float amount) {
            var st = MarketInventoryItemStructs.Get(index);
			switch(st.ItemType)
			{
				case ItemCategory.Ship:
				case ItemCategory.PlanetUpgrade:
					return true;
			}
			if (st.Amount - amount > -1)
			{
				st.Amount = st.Amount - amount;
				MarketInventoryItemStructs.Set(index, st);
				return true;
			}
			return false;
        }

        public bool SubtractResource(int resourceDefId, float amount) 
		{
            for(int i=0; i<MarketInventoryItemStructs.Length; i++)
			{
				var miis = MarketInventoryItemStructs.Get(i);
				if (miis.ResourceDefId == resourceDefId && miis.Amount >= amount)
				{
					miis.Amount -= amount;
					MarketInventoryItemStructs.Set(i, miis);
					return true;
				}
			}
			return false;
        }

        public bool SubtractResourcesForUpgrade(int planetUpgradeDefId, int level)
		{
			bool allPassed = true;
			var upgrade = Defs.GetPlanetUpgradeDef(planetUpgradeDefId);
			var upgmap = upgrade.GetLevelMap(level);
			foreach(var map in upgmap.CostOfUpgradeResourceAmountMaps)
			{
				if (!SubtractResource(map.ResourceDefId, map.Amount))
					allPassed = false;
			}
			return allPassed;
		}

		public MarketInventoryItemStruct GetMarketItemStruct(int index)
		{
			return MarketInventoryItemStructs.Get(index);
		}

		public int FindResourceIndex(int resId)
		{
            for(int i=0; i<MarketInventoryItemStructs.Length; i++)
			{
				var miis = MarketInventoryItemStructs.Get(i);
				if (miis.ResourceDefId == resId)
				{
					return i;
				}
			}
			return -1;
		}

		public void SetMarketItemStruct(int index, MarketInventoryItemStruct st)
		{
			var st2 = GetMarketItemStruct(index);
			st2.Fill(st);
			MarketInventoryItemStructs.Set(index, st);
		}

		public void RefreshMarketInventoryItemsFromStructs() {
			MarketInventoryItems.Clear();

			var structs = MarketInventoryItemStructs;
			for(int i=0; i<structs.Length; i++)
			{
				if (structs[i].ItemType == ItemCategory.None)
					continue;

				if (structs[i].ItemType == ItemCategory.Resource)
					continue;

				var mii = new MarketInventoryItem(structs[i], i);
				if (mii.ItemType == ItemCategory.PlanetUpgrade)
				{
					var upg = Defs.GetPlanetUpgradeDef(structs[i].PlanetUpgradeDefId);
					var level = GetPlanetUpgradeLevel(upg.Id);
					if (level >= PlanetManager.Instance.MaxPlanetUpgradeLevel)
						continue; // dont add to list
					mii.Amount = level + 1;
					mii.Price = upg.GetLevelPrice(level + 1);
				}
				MarketInventoryItems.Add(mii);
			}
			OnInventoryChanged?.Invoke();
		}

		public void RefreshTransportRouteFromStructs() {
			var upgrade = GetUpgrade(8);
			var route = new TransportRoute(TransportStopActionStructs);
			upgrade.SetRoute(route);
		}

		static void OnMarketInventoryItemsChanged(Changed<Planet> pl) 
		{
			if (!pl.Behaviour)
				return;

			var planet = pl.Behaviour;
			planet.RefreshMarketInventoryItemsFromStructs();
		}

		static void OnPlanetUpgradesChanged(Changed<Planet> pl) 
		{
			if (!pl.Behaviour)
				return;

			try
			{
				Planet planet = pl.Behaviour;
				planet.RefreshPlanetUpgradesFromStructs();
				planet.RefreshMarketInventoryItemsFromStructs();
				if (planet.GetComponent<PlanetVisualSimulation>())
					planet.GetComponent<PlanetVisualSimulation>().RefreshPlanetUpgradeVisuals(planet);
			}
			catch(System.Exception ex)
			{
				Debug.LogException(ex, pl.Behaviour);
			}
		}

		static void OnTransportStopActionsChanged(Changed<Planet> pl) 
		{
			if (!pl.Behaviour)
				return;

			var planet = pl.Behaviour;
			planet.RefreshTransportRouteFromStructs();
		}

		public void RefreshPlanetUpgradesFromStructs() {
			PlanetUpgrades.Where(x => x.Key != 0).ForEach(x => { 
				if (!Upgrades.ContainsKey(x.Key))
				{
					Debug.LogError($"RefreshPlanetUpgradesFromStructs failed {x.Key} {Upgrades.Count}");
					return;
				}
				var upgrade = GetUpgrade(x.Key);
				if (upgrade != null)
					upgrade.SetLevel(x.Value);
			});
		}

		public int GetPlanetUpgradeLevel(int upgradeId)
		{
			return PlanetUpgrades.Get(upgradeId);
		}

		public void SetPlanetUpgradeLevel(int upgradeId, int level)
		{
			for(int i=0; i<MarketInventoryItemStructs.Length; i++)
			{
				var miis = MarketInventoryItemStructs.Get(i);
				if (miis.PlanetUpgradeDefId == upgradeId)
				{
					miis.Amount = level;
					MarketInventoryItemStructs.Set(i, miis);
				}
			}
			PlanetUpgrades.Set(upgradeId, level);
		}

		public void SetPlanetUpgradeRoute(int upgradeId, TransportRoute route)
		{
			var stopActionStructs = route.CreateStructs();
			int index = 0;
			for(int i=0; i<TransportStopActionStructs.Length; i++)
			{
				if (index < stopActionStructs.Count)
					TransportStopActionStructs.Set(i, stopActionStructs[index++]);
				else
					TransportStopActionStructs.Set(i, TransportStopActionStruct.Empty);
			}
		}

		public bool CanAffordResource(int resourceId, float amount)
		{
            foreach(var miis in MarketInventoryItemStructs.Where(x => x.ItemType == ItemCategory.Resource))
			{
				if (miis.ResourceDefId == resourceId && miis.Amount >= amount)
					return true;

			}
			return false;
		}

		public void GenerateResources()
		{
			Dictionary<int, float> nonProducedAmounts = new Dictionary<int, float>();
			foreach(var upgrade in Upgrades.Values)
			{
				var planetUpgrade = upgrade.Def as PlanetUpgradeDefinition;
				var level = GetPlanetUpgradeLevel(planetUpgrade.Id);
				var map = planetUpgrade.GetLevelMap(level);
				if (map != null)
				{
					foreach(var boost in map.ProductionBoostMaps)
					{
						switch(boost.FactorOrAmount)
						{
							case FactorOrAmountEnum.Amount:
							{
								if (!Def.ClassDef.ProductionItems.Any(x => x.ResourceDef.Id == boost.ResourceDefId))
									nonProducedAmounts.Add(boost.ResourceDefId, boost.ProductionAmount);
							}
							break;
						}
					}
				}
			}

			for(int i=0; i<MarketInventoryItemStructs.Length; i++)
			{
				var st = GetMarketItemStruct(i);
				if (st.ItemType == ItemCategory.Resource)
				{
					if (nonProducedAmounts.ContainsKey(st.ResourceDefId))
						st.Amount += nonProducedAmounts[st.ResourceDefId];

					var prod = Def.ClassDef.ProductionItems.FirstOrDefault(x => x.ResourceDef.Id == st.ResourceDefId);
					if (prod != null)
					{
						var productionAmount = Random.Range(prod.AmountPerDayMin, prod.AmountPerDayMax);
						var factoredAmount = 0f;
						foreach(var upg in Upgrades.Values)
						{
							var planetUpgrade = upg.Def as PlanetUpgradeDefinition;
							var level = GetPlanetUpgradeLevel(planetUpgrade.Id);
							var upgmap = planetUpgrade.GetLevelMap(level);
							if (upgmap != null)
							{
								var factorMaps = upgmap.ProductionBoostMaps.Where(x => x.ResourceDefId == prod.ResourceDef.Id);
								factorMaps.ForEach(x => { factoredAmount = productionAmount * x.ProductionFactor; });
							}
						}

						if (factoredAmount > 0)
							st.Amount += factoredAmount;
						else
							st.Amount += productionAmount;
					}

					SetMarketItemStruct(i, st);

					if (st.ResourceDefId == 1 && !string.IsNullOrEmpty(PlayerId))
					{
						var owner = NetworkManager.GetPlayer(PlayerId);
						if (owner != null)
							owner.AddCredits(st.Amount);
					}
				}
			}
			RefreshMarketInventoryItemsFromStructs();
		}

		public void AddResource(int resId, float amt)
		{
			for(int i=0; i<MarketInventoryItemStructs.Length; i++)
			{
				var st = GetMarketItemStruct(i);
				if (st.ItemType == ItemCategory.Resource && st.ResourceDefId == resId)
				{
					st.Amount += amt;
					SetMarketItemStruct(i, st);
					break;
				}
			}
		}

		public Vector3 GetRandomSatellitePosition(float radiusFactor = 2) {
			var radius = GetComponentInChildren<SphereCollider>().radius;
			var ret = (Random.onUnitSphere * (radius * radiusFactor)) + transform.position;
			return ret;
		}

		public Vector3 GetRandomShipPosition(float radiusFactor = 3) {
			var radius = GetComponentInChildren<SphereCollider>().radius;
			return transform.RandomPointOnCircleFromCenter(radius * radiusFactor);
		}
	}
}
