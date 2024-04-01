using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Enemy
    {
        public int CurrentTick;

        public virtual string Name { get; }
        public virtual char Symbol { get; }
        public virtual int InitialMoveRate { get; }
        public virtual int InitialHealth { get; }

        public int MoveRate { get; private set; }
        public int Health { get; private set; }

        public Enemy() {
            Health = InitialHealth;
            MoveRate = InitialMoveRate;
        }
        public void Damage(Tower tower, int dmg)
        {
            Health = Math.Max(0, Health - dmg);
            LogManager.AddHit(this, tower, dmg);
        }

        public virtual void Die() {
            //Console.Beep();
            LogManager.AddDeath(this);
        }
    }
}
