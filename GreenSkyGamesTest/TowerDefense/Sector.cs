using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Sector 
    {
        public SectorType SectorType { get; private set; }
        public Tower Tower { get; private set; }
        public Enemy Enemy { get; private set; }
        public readonly Vector2Int Position;

        public Sector(Vector2Int vector)
        {
            Position = new Vector2Int(vector.x, vector.y);
        }
        public void Clear() {
            SectorType = SectorType.None;
            Tower = null;
            Enemy = null;
        }
        public void SetSector(Tower tower)
        {
            SectorType = SectorType.Tower;
            Tower = tower;
        }
        public void SetSector(Enemy enemy) 
        {
            SectorType = SectorType.Enemy;
            Enemy = enemy;
        }
    }
}
