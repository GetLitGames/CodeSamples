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
	public class PlanetUpgrade : Upgrade
	{
		public PlanetUpgrade() : base() { }
		public PlanetUpgrade(Planet planet, UpgradeDefinition upgradeDef, int level) : base(planet, upgradeDef, level)
		{
		}

		public override void FixedUpdateNetwork(NetworkRunner runner, IStation station)
		{
			base.FixedUpdateNetwork(runner, station);
		}
	}
}
