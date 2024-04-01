using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
	static public class LogManager
	{
		static List<string> _outputs = new List<string>();

		static public void AddDeath(Enemy enemy)
		{
			_outputs.Add($"{enemy.GetType().Name} died!");
		}
		static public void AddHit(Enemy enemy, Tower tower, int dmg)
		{
			_outputs.Add($"{enemy.GetType().Name} was hit by {tower.GetType().Name} for {dmg}");
		}
		static public void Clear() {
			_outputs.Clear();
		}

		static public string BuildOutput() {
			StringBuilder sb = new StringBuilder();
			foreach(var output in _outputs)
				sb.AppendLine(output);
			return sb.ToString();
		}
	}
}
