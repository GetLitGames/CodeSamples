using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Fusion;

namespace _Game
{
	[System.Serializable]
	public class MarketInventoryItem
	{
		public MarketItemTransactionType TransactionType;
		public ItemCategory ItemType;
		public ResourceDefinition ResourceDef;
		public PlanetUpgradeDefinition PlanetUpgradeDef;
		public ShipDefinition ShipDef;
		public SkillDefinition SkillDef;
		public float Price = 0;
		public float Amount = 1;
		public float CostFactor = 1;
		public int StructIndex = -1;

		public MarketInventoryItem() { }
		public MarketInventoryItem(MarketInventoryItemStruct miis, int index)
		{
			TransactionType = (MarketItemTransactionType)miis.TransactionType;
			ItemType = (ItemCategory)miis.ItemType;
			if (miis.ResourceDefId != 0)
				ResourceDef = Defs.GetResourceDef(miis.ResourceDefId);
			if (miis.PlanetUpgradeDefId != 0)
				PlanetUpgradeDef = Defs.GetPlanetUpgradeDef(miis.PlanetUpgradeDefId);
			if (miis.ShipDefId != 0)
				ShipDef = Defs.GetShipDef(miis.ShipDefId);
			if (miis.SkillDefId != 0)
				SkillDef = Defs.GetSkillDef(miis.SkillDefId);
			Price = (float)miis.Price;
			Amount = (float)miis.Amount;
			CostFactor = (float)miis.CostFactor;
			StructIndex = index;
		}

		public void Fill(ref MarketInventoryItemStruct st) {
			st.TransactionType = TransactionType;
			st.ItemType = ItemType;
			st.ResourceDefId = 0;
			if (ResourceDef)
				st.ResourceDefId = ResourceDef.Id;
			st.PlanetUpgradeDefId = 0;
			if (PlanetUpgradeDef)
				st.PlanetUpgradeDefId = PlanetUpgradeDef.Id;
			st.ShipDefId = 0;
			if (ShipDef)
				st.ShipDefId = ShipDef.Id;
			st.SkillDefId = 0;
			if (SkillDef)
				st.SkillDefId = SkillDef.Id;
			st.Price = Price;
			st.Amount = Amount;
			st.CostFactor = CostFactor;
		}

		public string Title
		{
			get
			{
				switch(ItemType)
				{
					case ItemCategory.Torpedo:
						return SkillDef.Title;
					case ItemCategory.Ship:
						return ShipDef.Title;
					case ItemCategory.Resource:
						return ResourceDef.Title;
					case ItemCategory.PlanetUpgrade:
						return PlanetUpgradeDef.Title;
				}
				return string.Empty;
			}
		}

		public static readonly byte[] memVector2 = new byte[MarketInventoryItemStruct.TotalMemSize];
		public static short Serialize(StreamBuffer outStream, object customobject)
		{
			MarketInventoryItem mii = customobject as MarketInventoryItem;
			lock (memVector2)
			{
				byte[] bytes = memVector2;
				int index = 0;
				Protocol.Serialize((int)mii.TransactionType, bytes, ref index);
				Protocol.Serialize((int)mii.ItemType, bytes, ref index);
				Protocol.Serialize((int)mii.ResourceDef.Id, bytes, ref index);
				Protocol.Serialize((int)mii.PlanetUpgradeDef.Id, bytes, ref index);
				Protocol.Serialize(mii.ShipDef.Id, bytes, ref index);
				Protocol.Serialize(mii.SkillDef.Id, bytes, ref index);
				Protocol.Serialize(mii.Price, bytes, ref index);
				Protocol.Serialize(mii.Amount, bytes, ref index);
				Protocol.Serialize(mii.CostFactor, bytes, ref index);
				outStream.Write(bytes, 0, MarketInventoryItemStruct.TotalMemSize);
			}

			return MarketInventoryItemStruct.TotalMemSize;
		}

		public static object Deserialize(StreamBuffer inStream, short length)
		{
			MarketInventoryItem mii = new MarketInventoryItem();
			lock (memVector2)
			{
				int index = 0;
				int desInt = 0;
				float desFloat = 0;

				inStream.Read(memVector2, 0, MarketInventoryItemStruct.TotalMemSize);
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.TransactionType = (MarketItemTransactionType)desInt;
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.ItemType = (ItemCategory)desInt;
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.ResourceDef = Defs.GetResourceDef(desInt);
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.PlanetUpgradeDef = Defs.GetPlanetUpgradeDef(desInt);
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.ShipDef = Defs.GetShipDef(desInt);
				Protocol.Deserialize(out desInt, memVector2, ref index); mii.SkillDef = Defs.GetSkillDef(desInt);
				Protocol.Deserialize(out desFloat, memVector2, ref index); mii.Price = desFloat;
				Protocol.Deserialize(out desFloat, memVector2, ref index); mii.Amount = desFloat;
				Protocol.Deserialize(out desFloat, memVector2, ref index); mii.CostFactor = desFloat;
			}

			return mii;
		}
	}
}
