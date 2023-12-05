using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Fusion;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace _Game
{
	public class PlanetManager : Singleton<PlanetManager>
	{
		public CompassNavigatorPro.CompassProPOI PlanetCompassPOIPrefab;
		public float SunLightSourceDistance = 2000f;
		public int MaxPlanetUpgradeLevel = 10;
		public int PlanetResourceGenerationEveryMins = 60;

		[InlineButton("AddPlanetDefs")]
		[SerializeField] public List<PlanetDefinition> PlanetDefs;
		[InlineButton("AddPlanetUpgradeDefs")]
		[SerializeField] public List<PlanetUpgradeDefinition> PlanetUpgradeDefs;
		[InlineButton("AddResourceDefs")]
		[SerializeField] public List<ResourceDefinition> ResourceDefs;

		List<int> ResourceGeneratedAtMins = new List<int>(); // this list just blocks us from processing something twice
		static List<Planet> Planets = new List<Planet>();

		void Start()
		{
			if (!GameManager.IsServer)
				return;
		}

		static public void AddPlanet(Planet p)
		{
			Planets.Add(p);
		}

		static public void RemovePlanet(Planet p)
		{
			Planets.Remove(p);
		}
		static public Planet GetPlanet(NetworkObjectGuid guid) {
			return Planets.FirstOrDefault(x => x.PlanetId == guid);
		}
		static public List<Planet> GetPlanets() {
			return Planets; 
		}

		public PlanetDefinition GetRandomPlanetDef() {
			return PlanetDefs.ElementAt(Random.Range(0, PlanetDefs.Count-1));
		}

#if UNITY_EDITOR
		void AddPlanetDefs()
		{
			PlanetDefs.Clear();
			var defs = CustomManager.GetAllDefs<PlanetDefinition>();
			PlanetDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
		void AddPlanetUpgradeDefs()
		{
			PlanetUpgradeDefs.Clear();
			var defs = CustomManager.GetAllDefs<PlanetUpgradeDefinition>();
			PlanetUpgradeDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
		void AddResourceDefs()
		{
			ResourceDefs.Clear();
			var defs = CustomManager.GetAllDefs<ResourceDefinition>();
			ResourceDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
#endif

		public void ProcessPlanets()
		{
			TimeSpan span = DateTime.UtcNow.Subtract(new DateTime(1970,1,1,0,0,0));
			int totalMinutes = (int)span.TotalMinutes;

			if (!ResourceGeneratedAtMins.Contains(totalMinutes))
			{
				ResourceGeneratedAtMins.Add(totalMinutes); // this just blocks us from processing something twice
				Planets.ForEach(x => x.GenerateResources());
			}
		}
	}
}
