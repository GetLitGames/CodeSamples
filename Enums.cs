using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game
{
	public class Enums
	{
	}

	public enum SortDirection
	{
		Ascending,
		Descending
	}
	public enum ButtonClickType
	{
		Multimedia,
		Plastic,
		Computer,
		Glass
	}

	public enum TargetingTypeEnum
	{
		Self,
		Character,
		PointBlankSphere,
		PointBlankCone,
		GroundTarget
	}

	public enum PositionalTypeEnum
	{
		None,
		Front,
		Behind,
		Side
	}

	public enum SkillEffectFunctionalType
	{
		DamageOrHeal,
		Stun,
		Summon
	}

	public enum RandomLootSpawnType
	{
		SpawnTable,
		ItemGroup,
		AlwaysOnly
	}

	public enum MobSpawnType
	{
		SpawnTable,
		SingleMob
	}

	public enum PortalTypeEnum
	{
		Entrance,
		Exit,
		StairsUp,
		StairsDown
	}

	public enum DifficultyEnum
	{
		Weak,
		Strong,
		Epic,
		MiniBoss,
		Boss
	}

	public enum FactionEnum 
	{
		None,
		Player,
		Neutral,
		Zilostet,
		Gronbarr,
		Federation,
		Scav,
		Arcan,
		NewOrder,
		Doftan,
		Total
	}

	public enum StanceEnum
	{
		Build,
		Defensive,
		Offensive
	}

	public enum EquipmentSlotEnum
	{
		Inventory,
		Head,
		Neck,
		Shoulders,
		Chest,
		Hands,
		Waist,
		Legs,
		Feet,
		Accessory1,
		Accessory2,
		Weapon,
		Offhand,
		Tool,
		Weapon2,
		Total // if you add more Inventory slots you need to go to the Bolt Assets Window and find Equipment in the Character State and increase the array length
	};

	public enum CharacterFleeType
	{
		None,
		AlwaysOnHit,
		LowHealth
	}

	public enum SkillSlot
	{
		None,
		Primary,
		Secondary,
		Torpedo
	}

	public enum StarbaseMainArea
	{
		None,
		Ships,
		Market,
		Loadout,
		Upgrades
	}

	public enum StationSubArea
	{
		None,
		Sell,
		Buy
	}

	public enum ItemCategory
	{
		None,
		Ship,
		Resource,
		PlanetUpgrade,
		Torpedo,
		StarbaseUpgrade,
		ShipUpgrade
	}

	public enum ResourceType {
		None,
		Electronics,
		Fuel,
		RawMaterial,

		Total
	}

	public enum MarketItemTransactionType
	{
		None,
		Buy,
		Sell
	}

	public enum PrefabDeployPoint
	{
		None,
		Center,
		Projectile
	}

	public enum PlanetUpgradeType
	{
		None,
		Colonies,
		Mines,
		Factories,
		Bunkers,
		SatelliteDrones, // 5
		PlanetaryRailGuns,
		Total
	}

	[Flags]
	public enum PauseFlags {
		None, 
		Input = 1, 
		Velocity = 2, 
		All = -1 
	}

	public enum ShipShowEffectType {
		None = 0,
		LaserFire = 1
	}

	public enum SoundEffectType {
		None = 0,
		EnergyHit = 1
	}

	[Flags]
	public enum TargetTypeFlag {
		None = 0,
		Planet = 1,
		Star = 2,
		ResourceField = 4
	}

	public enum TransportActionType {
		None,
		Load,
		Unload
	}

	public enum TransportState {
		Start,
		Unloading,
		Loading,
		Enroute,
		Docking
	}

	public enum HyperlinkType {
		None,
		Ship,
		Planet,
		Star
	}

	public enum ShipState {
		None,
		Docked,
		Dead
	}

	public enum FactorOrAmountEnum { 
		None, 
		Factor, 
		Amount 
	};

	public enum RespawnSelectionEnum {
		None,
		AtHomePlanet,
		RandomSystem
	}
}
