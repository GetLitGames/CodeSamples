using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Enemies
{
    public class Zombie : Enemy
    {
        public override char Symbol => 'z';
		public override int InitialMoveRate => 2;
		public override int InitialHealth => 10;
    }
}
