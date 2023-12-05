using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using Fusion;

namespace _Game
{
    public class ShipUpgrade : Upgrade
    {
        public ShipUpgrade(Planet planet, UpgradeDefinition upgradeDef, int level) : base(planet, upgradeDef, level)
        {
        }
    }
}
