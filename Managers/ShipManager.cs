using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace _Game
{
    public class ShipManager : Singleton<ShipManager>
    {
        public float ShipFuelConsumptionRate = .02f;

        public CompassNavigatorPro.CompassProPOI ShipCompassPOIPrefab;
        public GameObject ShipCameraPrefab;
        public GameObject ShipDamageNumbersPrefab;

        public List<ShipShowEffectMap> ShowEffects;
        [InlineButton("AddShipDefs")]
        public List<ShipDefinition> ShipDefs;
        [InlineButton("AddSkillDefs")]
        public List<SkillDefinition> SkillDefs;
        [InlineButton("AddPassiveDefs")]
        public List<PassiveDefinition> PassiveDefs;

        public AudioClip EnergyHitSound;

        static public List<Ship> Ships = new List<Ship>();

		#if UNITY_EDITOR
		void AddShipDefs()
		{
			ShipDefs.Clear();
			var defs = CustomManager.GetAllDefs<ShipDefinition>();
			ShipDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
        #endif
		#if UNITY_EDITOR
		void AddSkillDefs()
		{
			SkillDefs.Clear();
			var defs = CustomManager.GetAllDefs<SkillDefinition>();
			SkillDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
        #endif
		#if UNITY_EDITOR
		void AddPassiveDefs()
		{
			PassiveDefs.Clear();
			var defs = CustomManager.GetAllDefs<PassiveDefinition>();
			PassiveDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
        #endif

        void Start()
        {
            {
                var duplicates = ShipDefs.GroupBy(x => x.Id).Where(g => g.Count() > 1).ToList();
                foreach(var dup in duplicates)
                {
                    Debug.LogError($"Duplicate ShipDefinition Id found: {dup.Key}");
                }
            }
            {
                var duplicates = PassiveDefs.GroupBy(x => x.Id).Where(g => g.Count() > 1).ToList();
                foreach(var dup in duplicates)
                {
                    Debug.LogError($"Duplicate PassiveDefinition Id found: {dup.Key}");
                }
            }
        }

        static public void AddShip(Ship ship)
        {
            Ships.Add(ship);
        }

        static public void RemoveShip(Ship ship)
        {
            Ships.Remove(ship);
        }
    }

    [Serializable]
    public class ShipShowEffectMap {
        public ShipShowEffectType EffectType;
        public PrefabDeployPoint DeployPoint;
        public GameObject EffectPrefab;
    }
}
