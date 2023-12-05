using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace _Game
{
    static public class Defs
    {
        static public PlanetDefinition GetPlanetDef(int id)
        {
            return PlanetManager.Instance.PlanetDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<PlanetDefinition> GetPlanetDefs()
        {
            return PlanetManager.Instance.PlanetDefs;
        }
        static public StarbaseDefinition GetStarbaseDef(int id)
        {
            return StarManager.Instance.StarbaseDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<StarbaseDefinition> GetStarbaseDefs()
        {
            return StarManager.Instance.StarbaseDefs;
        }
        static public StarDefinition GetStarDef(int id)
        {
            return StarManager.Instance.StarDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<StarDefinition> GetStarDefs()
        {
            return StarManager.Instance.StarDefs;
        }
        static public ShipDefinition GetShipDef(int id) {
            if (!ShipManager.Instance.ShipDefs.Any(x => x.Id == id))
                Debug.LogError($"GetShipDefinition {id} not found, it needs to be added to Startup scene / ShipManager");
            return ShipManager.Instance.ShipDefs.FirstOrDefault(x => x.Id == id);
        }
        static public PassiveDefinition GetPassiveDef(int id) {
            if (!ShipManager.Instance.PassiveDefs.Any(x => x.Id == id))
                Debug.LogError($"GetPassiveDefinition {id} not found, it needs to be added to Startup scene / ShipManager");
            return ShipManager.Instance.PassiveDefs.FirstOrDefault(x => x.Id == id);
        }
        static public SkillDefinition GetSkillDef(int id) {
            if (!ShipManager.Instance.SkillDefs.Any(x => x.Id == id))
                Debug.LogError($"GetSkillDefinition {id} not found, it needs to be added to Startup scene / ShipManager");
            return ShipManager.Instance.SkillDefs.FirstOrDefault(x => x.Id == id);
        }
        static public PlanetUpgradeDefinition GetPlanetUpgradeDef(int id)
        {
            return PlanetManager.Instance.PlanetUpgradeDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<PlanetUpgradeDefinition> GetPlanetUpgradeDefs()
        {
            return PlanetManager.Instance.PlanetUpgradeDefs;
        }

        static public ResourceDefinition GetResourceDef(int id) {
            return PlanetManager.Instance.ResourceDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<ResourceDefinition> GetResourceDefs() {
            return PlanetManager.Instance.ResourceDefs;
        }
        static public POIDefinition GetPOIDef(int id) {
            return WorldManager.Instance.POIDefs.FirstOrDefault(x => x.Id == id);
        }
        static public List<POIDefinition> GetPOIDefs()
        {
            return WorldManager.Instance.POIDefs;
        }
    }
}
