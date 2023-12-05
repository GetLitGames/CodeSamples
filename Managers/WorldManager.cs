using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Fusion;
using _Game;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace _Game
{
	public class WorldManager : Singleton<WorldManager>
	{
		public int TotalStarSystems = 10;
		public int MinPlanetsPerStar = 2;
		public int MaxPlanetsPerStar = 6;
		public int PercentChanceForPOI = 20;
		public float StarSpreadDistance = 2000f;
		public float PlanetSpacingDistance = 600f;
		public int WorldXMin = -32000;
		public int WorldXMax = 32000;
		public float SpawnPointDistanceFromStar = 3000f;

        public MarketInventoryItem[] NewPlayerMarketItems;

		[InlineButton("AddPOIDefs")]
		public List<POIDefinition> POIDefs;

		static public WorldRecord CurrentWorld { get; set; }
		static public bool IsWorldLoaded { get; set; }

		void AddPOIDefs()
		{
#if UNITY_EDITOR
			POIDefs.Clear();
			var defs = CustomManager.GetAllDefs<POIDefinition>();
			POIDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
#endif
		}

		public void Load()
		{
			print($"WorldManager - Load..");
			LoadWorld("World1");
		}

		public void LoadWorld(string worldName)
		{
			print($"WorldManager - LoadWorld {worldName}");
			StartCoroutine(LoadCo(worldName));
		}

		IEnumerator LoadCo(string worldName) {
			print($"WorldManager - LoadCo {worldName}");
			var trPlanets = GameObject.FindGameObjectWithTag("Galaxy").transform;
			if (DataManager.Instance.IsWorldThere(worldName))
			{
				IsWorldLoaded = false;
				var worldRec = DataManager.Instance.LoadWorldFile(worldName);
				if (worldRec != null && worldRec.Planets.Count > 0)
				{
					foreach (var starRec in worldRec.Stars)
					{
						var networkObject = NetworkManager.Instance.Spawn<StarDefinition>(PlayerRef.None, Defs.GetStarDef(starRec.StarDefId), starRec.Position);
						networkObject.transform.SetParent(trPlanets);
						yield return null;
					}

					yield return null;
					foreach (var planetRec in worldRec.Planets)
					{
						var networkObject = NetworkManager.Instance.Spawn<PlanetDefinition>(PlayerRef.None, Defs.GetPlanetDef(planetRec.PlanetDefId), planetRec.Position, planetRec.PlayerId);
						var planet = networkObject.GetComponent<Planet>();

						System.Guid.TryParse(planetRec.PlanetId, out var planetId);
						planet.PlanetId = planetId;
						planet.SetPlayerId(planet.PlayerId);

						networkObject.transform.SetParent(trPlanets);
						yield return null;

						for(int i=0; i<planetRec.MarketItems.Count; i++)
						{
							planet.SetMarketItemStruct(i, planetRec.MarketItems[i]);
						}
						yield return null;

						foreach(var upg in planetRec.Upgrades)
						{
							planet.SetPlanetUpgradeLevel(upg.UpgradeDefId, upg.Level);
							if (upg.Route != null && upg.Route.Stops != null && upg.Route.Stops.Count > 0)
							{
								yield return new WaitForSecondsRealtime(1f);
								planet.SetPlanetUpgradeRoute(upg.UpgradeDefId, upg.Route);

								var upgrade = planet.GetUpgrade(upg.UpgradeDefId);
								upgrade.SetActive(upg.IsActive);
							}
						}
					}

					worldRec.Ships.ForEach(ship => {
						NetworkManager.Instance.SpawnShip(PlayerRef.None, Defs.GetShipDef(ship.ShipDefId), ship.Position, Quaternion.identity);
					});

					worldRec.Starbases.ForEach(sb => {
						NetworkManager.Instance.Spawn<StarbaseDefinition>(PlayerRef.None, Defs.GetStarbaseDef(sb.StarbaseDefId), sb.Position);
					});

					int count = 0;
					worldRec.SpawnPoints.ForEach(sp => {
						var go = new GameObject($"SpawnPoint {count++}");
						go.transform.position = sp;
						go.AddComponent<SpawnPoint>();
					});

					//worldRec.Players.ForEach(playerRec => {
					//	var player = NetworkManager.GetPlayer(playerRec.PlayerId);
					//	if (playerRec.PlayerId == player.PlayerId)
					//	{
					//		player.Credits = playerRec.Credits;
					//		if (!string.IsNullOrEmpty(playerRec.HomePlanetId))
					//			player.HomePlanetNetworkObject = PlanetManager.GetPlanet(new NetworkObjectGuid(playerRec.HomePlanetId)).NetworkObject;

					//		player.MakeShipActiveRpc(player.Object, playerRec.ShipDefId);
					//	}
					//});

					CurrentWorld = worldRec;
					IsWorldLoaded = true;
				}
				else
				{
					GenerateGalaxy();
				}
			}
			else
			{
				GenerateGalaxy();
			}

			yield return new WaitUntil(() => IsWorldLoaded);

			print($"WorldManager - Load Done");
			GameManager.Instance.SetState(GameManager.GameManagerState.LoadWorldDone);
		}

		public PlayerRecord GetPlayerRec(Player player) {
			if (CurrentWorld == null)
			{
				Debug.LogError($"LoadPlayerRec no current world!");
				return null;
			}

			return CurrentWorld.Players.FirstOrDefault(x => x.PlayerId == player.PlayerId);
		}

		public void Save() {
			print($"WorldManager - Save..");
			SaveWorld("World1");
			GameManager.Instance.SetState(GameManager.GameManagerState.SaveWorldDone);
		}

		public WorldRecord SaveWorld(string worldName) 
		{
			print($"WorldManager - SaveWorld {worldName}");
			var worldRec = new WorldRecord(worldName);

			NetworkManager.Players.ForEach(x => worldRec.Players.Add(new PlayerRecord(x.Value)));
			ShipManager.Ships.Where(x => !x.IsPlayer).ForEach(ship => worldRec.Ships.Add(new ShipRecord(ship)));
			GameObject.FindObjectsOfType<Starbase>().ForEach(sb => worldRec.Starbases.Add(new StarbaseRecord(sb)));
			GameObject.FindObjectsOfType<SpawnPoint>().ForEach(sp => worldRec.SpawnPoints.Add(sp.transform.position));

			var stars = StarManager.Instance.GetStars();
			var starRecs = new List<StarRecord>(stars.Count);
			foreach (var star in stars) {
				var starRecord = new StarRecord(star);
				worldRec.Stars.Add(starRecord);
			}

			PlanetManager.GetPlanets().ForEach(planet => worldRec.Planets.Add(new PlanetRecord(planet)));

			DataManager.Instance.SaveWorldFile(worldName, worldRec);
			print($"WorldManager - SaveWorld Done");
			return worldRec;
		}

		void GenerateGalaxy() {
			IsWorldLoaded = false;
			print($"WorldManager - GenerateGalaxy");
			StartCoroutine(GenerateGalaxyCo());
		}

		IEnumerator GenerateGalaxyCo() {
			print($"WorldManager - GenerateGalaxyCo");
			yield return new WaitForSecondsRealtime(.15f);
			var trPlanets = GameObject.FindGameObjectWithTag("Galaxy").transform;

			System.Random rnd = new System.Random();

			var planetDefs = Defs.GetPlanetDefs();
			var poiDefs = Defs.GetPOIDefs();
			var starDefs = Defs.GetStarDefs();

			for(int i=0; i < TotalStarSystems; i++)
			{
				var distanceFromCenter = Random.Range(0, WorldXMax);
				int result = distanceFromCenter % 1000 >= 500 ? distanceFromCenter + 1000 - distanceFromCenter % 1000 : distanceFromCenter - distanceFromCenter % 1000;
				float factorX = Random.Range(0,1) == 1 ? 1f : -1f;
				float factorZ = Random.Range(0,1) == 1 ? 1f : -1f;
				Vector3 centerPos = new Vector3(distanceFromCenter * factorX, 0, distanceFromCenter * factorZ);

				bool isBlackHole = Random.Range(0, 100) < 15;
				var starDef = Defs.GetStarDef(isBlackHole ? 1 : 2);
				Vector3 pos = Vector3.zero;

				print($"WorldManager - Generate Star {starDef}");
				yield return new WaitForSecondsRealtime(.15f);

				Vector3 starPos = Vector3.zero;
				for (var attempts=0; attempts < 1000; attempts++)
				{
					starPos = centerPos.RandomPointOnCircleFromCenter(WorldXMax); //new Vector3(rnd.Next(WorldXMin, WorldXMax), 0, rnd.Next(WorldXMin, WorldXMax));
					var nearbyColliders = Physics.OverlapSphere(starPos, StarSpreadDistance, GameManager.Instance.StarsLayerMask);
					if (nearbyColliders.Length < 1)
						break;
				}

				yield return new WaitForSecondsRealtime(.15f);
				var networkObject = NetworkManager.Instance.Spawn<StarDefinition>(PlayerRef.None, starDef, starPos);
				networkObject.transform.SetParent(trPlanets);

				if (isBlackHole)
					continue;

				for (var attempts=0; attempts < 1000; attempts++)
				{
					pos = starPos.RandomPointOnCircleFromCenter(SpawnPointDistanceFromStar);
					var nearbyColliders = Physics.OverlapSphere(pos, PlanetSpacingDistance, GameManager.Instance.StarsAndPlanetsLayerMask);
					if (nearbyColliders.Length < 1)
						break;
				}
				yield return new WaitForSecondsRealtime(.15f);

				var go = new GameObject("SpawnPoint");
				go.transform.position = pos;
				go.AddComponent<SpawnPoint>();

				var totalPlanets = rnd.Next(MinPlanetsPerStar, MaxPlanetsPerStar);
				for(int j=0; j < totalPlanets+1; j++)
				{
					pos = starPos.RandomPointOnCircleFromCenter(PlanetSpacingDistance * (j+3)); // we don't need to do overlapping because we make sure things are spaced out
					if (j != 0)
					{
						if (rnd.Next(0, 100) >= PercentChanceForPOI)
						{
							var planetDef = planetDefs[rnd.Next(0, planetDefs.Count)];
							networkObject = NetworkManager.Instance.Spawn<PlanetDefinition>(PlayerRef.None, planetDef, pos, string.Empty);
							networkObject.transform.SetParent(trPlanets);
							yield return null;
						}
						else // 10 percent chance for each planet to be a POI
						{
							var poiDef = poiDefs[rnd.Next(0, poiDefs.Count)];
							NetworkManager.Instance.SpawnPOI(PlayerRef.None, poiDef, pos, string.Empty);
							yield return null;
						}
					}
					else // one starbase per system
					{
						var def = Defs.GetStarbaseDef(1);
						networkObject = NetworkManager.Instance.Spawn<StarbaseDefinition>(PlayerRef.None, def, pos);
					}
					yield return new WaitForSecondsRealtime(.15f);
				}
			}
			yield return new WaitForSecondsRealtime(.5f);
			CurrentWorld = SaveWorld("Temp");
			yield return new WaitForSecondsRealtime(.5f);
			IsWorldLoaded = true;
		}
	}
}
