using System.Collections;
using System.Collections.Generic;
using Asteroids.HostSimple;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System.Linq;

namespace _Game
{
	public class Player : NetworkBehaviour
	{
		public PlayerRef PlayerRef;

		[Networked] public NetworkString<_32> DisplayName { get; set; }
		[Networked] public NetworkString<_32> PlayerId { get; set; }
		[Networked] public float Credits { get; set; }
		[Networked] public int ShipDefId { get; set; }
		[Networked] public NetworkObject ShipNetworkObject { get; set; }
		[Networked] public NetworkObject HomePlanetNetworkObject { get; set; }
		[Networked, Capacity(32)] public NetworkArray<int> ShipDefIds { get; }

		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }

		public override void Spawned()
		{
			// --- Client
			// Find the local non-networked PlayerData to read the data and communicate it to the Host via a single RPC 
			if (Object.HasInputAuthority)
			{
				LocalPlayer.Player = this;

				var nickName = LocalPlayer.DisplayName;
				SetDisplayNameRpc(nickName);
				SetPlayerIdRpc(nickName);
			}

			// --- Host
			// Initialized game specific settings
			if (!GameManager.IsServer)
				return;
		}

		public float AddCredits(float credits) {
			Credits += credits;
			return Credits;
		}

		public float SubtractCredits(float credits) {
			if (Credits < credits)
				return -1;

			Credits -= credits;
			return Credits;
		}

		public bool CanAfford(float credits) {
			if (Credits >= credits)
				return true;
			return false;
		}

		// RPC used to send player information to the Host
		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		void SetDisplayNameRpc(string name)
		{
			if (string.IsNullOrEmpty(name)) return;
			DisplayName = name;
		}

		// RPC used to send player information to the Host
		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		void SetPlayerIdRpc(string id)
		{
			if (string.IsNullOrEmpty(id)) return;
			PlayerId = id;
		}

		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		public void SetRespawnSelectionRpc(RespawnSelectionEnum sel)
		{
			ShipNetworkObject.GetComponent<Ship>().Respawn(sel);
		}

		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		public void SetHomePlanetRpc(NetworkObject planetNetworkObject)
		{
			HomePlanetNetworkObject = planetNetworkObject;
		}

		[Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
		public void ShowPanelRpc(string panelName)
		{
			UIManager.ShowPanel(panelName);
		}

		//[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		//public void BuyShipRpc(PlayerRef playerRef, NetworkObject networkObj, int defId)
		//{
		//	if (defId < 1)
		//		return;

		//	ShipDefIds.Set(ShipDefIdsTotal, defId);
		//	ShipDefIdsTotal++;

		//	var shipDef = ShipManager.Instance.GetShipDef(defId);
		//	var player = NetworkManager.Instance.GetPlayer(playerRef);
		//	if (player.CanAfford(shipDef.Value))
		//	{
		//		player.SubtractCredits(cost);
		//		planet.AddPlanetUpgrade(upg);
		//	}
		//}

		//[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		//public void BuyPlanetUpgradeRpc(PlayerRef playerRef, NetworkObject networkObj, int defId)
		//{
		//	if (!GameManager.IsServer)
		//		return;

		//	var upg = PlanetManager.Instance.GetPlanetUpgradeDef(defId);
		//	Planet planet = networkObj.GetComponent<Planet>();
		//	var upgLevel = planet.GetPlanetUpgradeLevel(upg);
		//	var cost = upg.GetPrice(upgLevel);
		//	var player = NetworkManager.Instance.GetPlayer(playerRef);
		//	if (player.CanAfford(cost))
		//	{
		//		player.SubtractCredits(cost);
		//		planet.AddPlanetUpgrade(upg);
		//	}
		//}

		public bool CanAffordMarketItemStruct(IStation station, int marketItemIndex)
		{
			var item = station.GetMarketItemStruct(marketItemIndex);
			if (item.ItemType == ItemCategory.PlanetUpgrade)
			{
				var upg = Defs.GetPlanetUpgradeDef(item.PlanetUpgradeDefId);
				var planet = station as Planet;
				bool allPassed = true;
				var level = planet.GetPlanetUpgradeLevel(upg.Id);
				var upgmap = upg.GetLevelMap(level+1);
				foreach(var levelMap in upgmap.CostOfUpgradeResourceAmountMaps)
				{
					foreach(var map in upgmap.CostOfUpgradeResourceAmountMaps)
					{
						if (!planet.CanAffordResource(map.ResourceDefId, map.Amount))
							allPassed = false;
					}
				}
				if (!allPassed)
					return false;
			}
			return true;
		}

		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		public void BuyMarketItemRpc(PlayerRef playerRef, NetworkObject networkObj, int marketItemIndex, float amount)
		{
			if (!GameManager.IsServer)
				return;

			var player = NetworkManager.GetPlayer(playerRef);
			IStation station = networkObj.GetComponent<IStation>();
			Planet planet = networkObj.GetComponent<Planet>();

			var item = station.GetMarketItemStruct(marketItemIndex);
			var cost = item.Price;
			if (!player.CanAfford(cost) || !player.CanAffordMarketItemStruct(station, marketItemIndex))
				return;

			if (!station.SubtractMarketItem(marketItemIndex, amount))
				return;

			if (player.SubtractCredits(cost) < 0)
				return;

			if (item.ItemType == ItemCategory.PlanetUpgrade)
			{
				var currentLevel = planet.GetPlanetUpgradeLevel(item.PlanetUpgradeDefId);
				if (!planet.SubtractResourcesForUpgrade(item.PlanetUpgradeDefId, currentLevel+1))
					return;
			}

			switch(item.ItemType)
			{
				case ItemCategory.Ship:
				{
					if (!ShipDefIds.Any(x => x == item.ShipDefId))
					{
						for(int i=0; i<ShipDefIds.Length; i++)
						{
							if (ShipDefIds[i] == 0)
							{
								ShipDefIds.Set(i, item.ShipDefId);
								break;
							}
						}
					}
				}
				break;

				case ItemCategory.PlanetUpgrade:
				{
					var currentLevel = planet.GetPlanetUpgradeLevel(item.PlanetUpgradeDefId);
					planet.SetPlanetUpgradeLevel(item.PlanetUpgradeDefId, currentLevel+1);
				}
				break;

				default:
				{
					var ship = player.ShipNetworkObject.GetComponent<Ship>();
					ship.AddMarketItem(station.GetMarketItemStruct(marketItemIndex));
				}
				break;
			}

		}

		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
		public void MakeShipActiveRpc(NetworkObject playerObject, int shipDefId)
		{
			if (!GameManager.IsServer)
				return;

			if (shipDefId < 1)
				return;

			var player = playerObject.GetComponent<Player>();

			var shipDef = Defs.GetShipDef(shipDefId);
			var oldPosition = player.Position;
			var oldRotation = player.Rotation;
			
			if (player.ShipNetworkObject != null)
			{
				oldPosition = player.ShipNetworkObject.transform.position;
				oldRotation = player.ShipNetworkObject.transform.rotation;
				player.Position = oldPosition;
				player.Rotation = oldRotation;
				NetworkManager.Instance.Despawn(player.ShipNetworkObject);
			}
			NetworkManager.Instance.SpawnShip(playerObject.InputAuthority, shipDef, oldPosition, oldRotation, player.DisplayName.Value);
		}

		[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
		public void SetUpgradeActiveRpc(NetworkObjectGuid planetId, int upgradeDefId, bool active)
		{
			var planet = PlanetManager.GetPlanet(planetId);
			var upgrade = planet.GetUpgrades()[upgradeDefId];
			upgrade.SetActive(active);
		}
	}
}
