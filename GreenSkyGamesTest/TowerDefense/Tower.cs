using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Tower
    {
        public int CurrentTick;

        public virtual char Symbol { get; }
        public virtual int AttackRadius { get; }
        public virtual int FireRate { get; }
        public virtual int Damage { get; }
        public virtual int Health { get; }
        public Vector2Int Position { get; set; }

        protected Tower(Vector2Int position)
        {
            Position = position;
        }

        public void Attack(Enemy target)
        {
            target.Damage(this, Damage);
            if (target.Health <= 0)
                target.Die();
        }
    }
}
