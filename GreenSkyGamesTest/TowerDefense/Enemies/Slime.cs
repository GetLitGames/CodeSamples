using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Enemies
{
    public class Slime : Enemy
    {
        public override char Symbol => 's';
		public override int InitialMoveRate => 1;
		public override int InitialHealth => 3;
    }
}
