using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Towers
{
    public class Cannon : Tower
    {
        public override char Symbol => 'C';
        public override int AttackRadius => 5;
        public override int FireRate => 3;
        public override int Damage => 1;
        public override int Health => 3;

        public Cannon(Vector2Int position) : base(position) { }
    }
}
