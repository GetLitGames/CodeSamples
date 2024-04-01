using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
	public class Enums { }

	public enum TowerType { 
		None,
		Assault,
		Bombard,
		Cannon
	}
	public enum EnemyType {
		None,
		Slime,
		Tank,
		Zombie,

		Max
	}
	public enum SectorType {
		None,
		Tower,
		Enemy
	}
}
