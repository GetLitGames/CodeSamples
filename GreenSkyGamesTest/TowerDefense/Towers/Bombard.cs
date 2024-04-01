using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Towers
{
    public class Bombard : Tower
    {
        public override char Symbol => 'B';
        public override int AttackRadius => 4;
        public override int FireRate => 2;
        public override int Damage => 2;
        public override int Health => 5;

        public Bombard(Vector2Int position) : base(position) { }
    }
}
