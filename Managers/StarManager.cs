using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace _Game
{
    public class StarManager : Singleton<StarManager>
    {
        public CompassNavigatorPro.CompassProPOI StarCompassPOIPrefab;
        public CompassNavigatorPro.CompassProPOI StarbaseCompassPOIPrefab;
        public float MaxLightDistance = 1000f;
        public float LightIntensityStepSize = 100f;
        public float MaxLightIntesity = 5f;

        [InlineButton("AddStarDefs")]
        public List<StarDefinition> StarDefs;
        [InlineButton("AddStarbaseDefs")]
        public List<StarbaseDefinition> StarbaseDefs;

        List<Star> Stars = new List<Star>();
        List<Starbase> Starbases = new List<Starbase>();

		#if UNITY_EDITOR
		void AddStarDefs()
		{
			StarDefs.Clear();
			var defs = CustomManager.GetAllDefs<StarDefinition>();
			StarDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
        #endif

		#if UNITY_EDITOR
		void AddStarbaseDefs()
		{
			StarbaseDefs.Clear();
			var defs = CustomManager.GetAllDefs<StarbaseDefinition>();
			StarbaseDefs.AddRange(defs);
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}
        #endif

        public void AddStar(Star star)
        {
            Stars.Add(star);
        }

        public void RemoveStar(Star star)
        {
            Stars.Remove(star);
        }

        public List<Star> GetStars() {
            return Stars;
        }

        void Update()
        {
            if (!GameManager.IsServer)
                return;
        }
    }
}
