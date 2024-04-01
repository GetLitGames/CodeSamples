using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense
{
    public struct Vector2Int
    {
        public int x { get; private set; }
        public int y { get; private set; }

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

		public override string ToString()
		{
			return $"{x},{y}";

        }

        public double DistanceTo(Vector2Int other)
        {
            int deltaX = other.x - this.x;
            int deltaY = other.y - this.y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
	}
}
