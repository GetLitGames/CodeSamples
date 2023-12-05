using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Fusion;

namespace _Game
{
	[System.Serializable]
	public struct MarketInventoryItemStruct : INetworkStruct
	{
		public MarketItemTransactionType TransactionType;
		public ItemCategory ItemType;
		public int ResourceDefId;
		public int PlanetUpgradeDefId;
		public int ShipDefId;
		public int SkillDefId;
		public float Price;
		public float Amount;
		public float CostFactor;

		public static short TotalMemSize = 45; // generally each one adds 5 bytes
		public static MarketInventoryItemStruct Empty = default;

		public MarketInventoryItemStruct(MarketInventoryItemStruct st) {
			this.TransactionType = st.TransactionType;
			this.ItemType = st.ItemType;
			this.ResourceDefId = st.ResourceDefId;
			this.PlanetUpgradeDefId = st.PlanetUpgradeDefId;
			this.ShipDefId = st.ShipDefId;
			this.SkillDefId = st.SkillDefId;
			this.Price = st.Price;
			this.Amount = st.Amount;
			this.CostFactor = st.CostFactor;
		}

		public void Fill(MarketInventoryItemStruct st)
		{
			TransactionType = st.TransactionType;
			ItemType = st.ItemType;
			ResourceDefId = st.ResourceDefId;
			PlanetUpgradeDefId = st.PlanetUpgradeDefId;
			ShipDefId = st.ShipDefId;
			SkillDefId = st.SkillDefId;
			Price = st.Price;
			Amount = st.Amount;
			CostFactor = st.CostFactor;
		}
	}

	[System.Serializable]
	public struct PlanetUpgradeStruct : INetworkStruct
	{
		public NetworkBool IsActive;
		public int PlanetUpgradeDefId;

		public static short TotalMemSize = 10; // generally each one adds 5 bytes
		public static PlanetUpgradeStruct Empty = default;
	}

	[System.Serializable]
	public struct TransportStopActionStruct : INetworkStruct
	{
		public NetworkObjectGuid PlanetId;
		public TransportActionType Action;
		public int ResourceDefId;

		public static short TotalMemSize = 15; // generally each one adds 5 bytes
		public static TransportStopActionStruct Empty = default;
		public static TransportStopActionStruct Defaults
		{
			get
			{
				var result = new TransportStopActionStruct();
				result.Action = TransportActionType.Load;
				return result;
			}
		}	
	}
}
