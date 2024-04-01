using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Enemies
{
    public class Tank : Enemy
    {
        public override char Symbol => 't';
		public override int InitialMoveRate => 3;
		public override int InitialHealth => 20;
    }
}
