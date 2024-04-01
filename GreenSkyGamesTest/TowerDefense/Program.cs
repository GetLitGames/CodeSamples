using System.Runtime.CompilerServices;
using TowerDefense;

namespace TowerDefense
{
    public class Program
    {
        static int Main(string[] args)
        {
            bool gameIsOver = false;

            Game game = new Game(new Vector2Int(8, 8),
                new List<Vector2Int>()
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2),
                    new Vector2Int(3, 2),
                    new Vector2Int(3, 3),
                    new Vector2Int(4, 3),
                    new Vector2Int(4, 4),
                    new Vector2Int(5, 4),
                    new Vector2Int(5, 5),
                    new Vector2Int(6, 5),
                    new Vector2Int(6, 6),
                    new Vector2Int(7, 6),
                    new Vector2Int(7, 7),
                },
                () => gameIsOver = true
                );

            game.Spawn(TowerType.Assault, new Vector2Int(0, 2));
            game.Spawn(TowerType.Assault, new Vector2Int(0, 4));
            game.Spawn(TowerType.Assault, new Vector2Int(1, 2));
            game.Spawn(TowerType.Assault, new Vector2Int(5, 3));
            game.Spawn(TowerType.Assault, new Vector2Int(5, 4));

            game.Spawn(TowerType.Bombard, new Vector2Int(2, 4));
            game.Spawn(TowerType.Bombard, new Vector2Int(6, 4));

            game.Spawn(TowerType.Cannon, new Vector2Int(3, 5));
            game.Spawn(TowerType.Cannon, new Vector2Int(7, 5));


            while (!gameIsOver)
            {
                if (game.CanSpawnEnemy())
                {
                    Random random = new Random();
                    int enemyType = random.Next(0, (int)EnemyType.Max);
                    game.Spawn((EnemyType)enemyType, Game.StartingVector);
                }

                game.Update();

                PrintBoard(game);

                Thread.Sleep(200);
            }

            Console.WriteLine("GAME OVER!");

            return 0;
        }
        private static void PrintBoard(Game game)
        {
            Console.Clear();
            Console.WriteLine("BOARD:");

            for (int j = 0; j < game.WorldSize.y; j++)
            {
                for (int i = 0; i < game.WorldSize.x; i++)
                    Console.Write(' ' + game.GetDisplayChar(new Vector2Int(i, j)).ToString());

                Console.Write("\n");
            }

            Console.Write("\n");

            var output = LogManager.BuildOutput();
            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine(output);
                LogManager.Clear();
            }
        }
    }
}


