using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public class Game
    {
        public Vector2Int WorldSize { get; private set; }
        List<Sector> _sectors;
        List<Vector2Int> _enemyPath;
        static public Vector2Int StartingVector = new Vector2Int(0,0);

        public Game(Vector2Int worldSize, List<Vector2Int> enemyPath, Action onGameOver)
        {
            WorldSize = worldSize;
            _enemyPath = enemyPath;

            int len = worldSize.x * worldSize.y;
            _sectors = new List<Sector>(len);
            for(int i=0; i<len; i++)
            {
                var position = GetCoordinatesByIndex(worldSize.x, i);
                _sectors.Add(new Sector(position));
            }
        }

        public void Update()
        {
            MoveEnemies();
            AttackWithTowers();
        }

        void MoveEnemies()
        {
            var enemySectors = _sectors.Where(x => x.SectorType == SectorType.Enemy);
            foreach(var enemySector in enemySectors)
            {
                var enemy = enemySector.Enemy;
                enemy.CurrentTick++;

                if (enemy.CurrentTick >= enemy.MoveRate)
                {
                    enemy.CurrentTick = 0;

                    var vector = _enemyPath.First(x => x.x == enemySector.Position.x && x.y == enemySector.Position.y);
                    var sectorIndex = _enemyPath.IndexOf(vector);
                    if (sectorIndex + 1 >= _enemyPath.Count)
                    {
                        // reached end of path!
                        enemySector.Clear();
                    }
                    else
                    {
                        var nextSector = GetSectorByCoordinates(_enemyPath[sectorIndex+1]);
                        if (nextSector.SectorType == SectorType.None) // only allow move if the next sector is empty
                        {
                            nextSector.SetSector(enemy);
                            enemySector.Clear();
                        }
                    }
                }
            }
        }

        void AttackWithTowers()
        {
            var towerSectors = _sectors.Where(x => x.SectorType == SectorType.Tower && x.Tower != null);
            foreach(var towerSector in towerSectors)
            {
                var tower = towerSector.Tower;
                tower.CurrentTick++;

                if (tower.CurrentTick >= tower.FireRate)
                {
                    tower.CurrentTick = 0;

                    var attackSectors = GetSectorsWithinRadius(towerSector.Position, tower.AttackRadius); // should add code to sort this byt the sectors furthest down the path
                    foreach(var attackSector in attackSectors)
                    {
                        if (attackSector.SectorType == SectorType.Enemy && attackSector.Enemy != null)
                        {
                            var enemy = attackSector.Enemy;
                            tower.Attack(enemy);
                            if (enemy.Health <= 0)
                            {
                                attackSector.Clear();
                            }
                            break; // can only hit once per tick
                        }
                    }
                }
            }
        }

        List<Sector> GetSectorsWithinRadius(Vector2Int position, int radius)
        {
            int width = WorldSize.x;
            int height = WorldSize.y;
            int centerX = position.x;
            int centerY = position.y;

            // Find points within the radius from the center
            List<Sector> sectorsWithinRadius = new List<Sector>();
            foreach (var sector in _sectors)
            {
                double distance = position.DistanceTo(sector.Position);
                if (distance <= radius)
                {
                    sectorsWithinRadius.Add(sector);
                }
            }

            return sectorsWithinRadius;
        }

        public void Spawn(TowerType type, Vector2Int position)
        {
			Tower tower = null;

			switch(type)
			{
				case TowerType.Assault:
					tower = new Towers.Assault(position);
					break;
				case TowerType.Cannon:
					tower = new Towers.Cannon(position);
					break;
				case TowerType.Bombard:
					tower = new Towers.Bombard(position);
					break;
			}
            if (tower == null)
                return;

            var sector = GetSectorByCoordinates(position);
            if (sector.SectorType != SectorType.None)
            {
                throw new ApplicationException($"Sector {position} already occupied by {sector.SectorType}");
            }
            else
			    sector.SetSector(tower);
        }

        public void Spawn(EnemyType type, Vector2Int position)
        {
			Enemy enemy = null;

			switch(type)
			{
				case EnemyType.Slime:
					enemy = new Enemies.Slime();
					break;
				case EnemyType.Tank:
					enemy = new Enemies.Tank();
					break;
				case EnemyType.Zombie:
					enemy = new Enemies.Zombie();
					break;
			}
            if (enemy == null) 
                return;

            var sector = GetSectorByCoordinates(position);
            if (sector.SectorType != SectorType.None)
            {
                throw new ApplicationException($"Sector {position} already occupied by {sector.SectorType}");
            }
            else
			    sector.SetSector(enemy);
        }

        Sector GetSectorByCoordinates(Vector2Int position)
        {
            int width = WorldSize.x;
            int x = position.x;
            int y = position.y;

            if (x >= 0 && x < width && y >= 0 && y < _sectors.Count / width)
            {
                int index = y * width + x;
                return _sectors[index];
            }
            return null; // Sector not found
        }

        Vector2Int GetCoordinatesByIndex(int width, int index)
        {
            int y = index / width;
            int x = index % width;
            return new Vector2Int(x, y);
        }

        public bool CanSpawnEnemy()
        {
            var startingSector = GetSectorByCoordinates(StartingVector);
            if (startingSector.SectorType != SectorType.None)
                return false;

            return true;
        }


        public char GetDisplayChar(Vector2Int position)
        {
            try
            {
                var sector = GetSectorByCoordinates(position);
                if (sector.Enemy != null || sector.Tower != null)
                {
                    switch(sector.SectorType)
                    {
                        case SectorType.Enemy:
                            return sector.Enemy.Symbol;
                        case SectorType.Tower:
                            return sector.Tower.Symbol;
                    }
                }
            }
            catch(System.Exception ex)
            {
            }

            var isPath = _enemyPath.Exists(x => x.x == position.x && x.y == position.y);
            if (isPath)
                return '_';

            return '.';
        }
    }
}
