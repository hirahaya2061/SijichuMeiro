using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Meiro
{
    public class Program
    {
        /// Entry Point
        public static void Main(string[] args)
        {
            var instance = new Meiro(15, 15);
            instance.GenerateMeiro();
            Console.WriteLine(instance.ExpressAsString());
        }
    }

    // 迷路クラス
    public class Meiro
    {
        // constant values
        const int WALL = 0;
        const int PATH = 1;
        public enum Direction
        {
            UP, RIGHT, DOWN, LEFT
        }

        // instance value 
        public int[,] Maze;
        public int Width;
        public int Height;
        public Cell Start;
        public Cell End;
        public int Seed;
        private Random random;
        public List<Cell> Paths = new List<Cell>();

        public Meiro(int width, int height, int seed=-1)
        {
            if(seed == -1)
                seed = new Random().Next();

            // [width], [height]が5未満なら強制的に5に指定
            if(width < 5 || height < 5)
            {
                width = 5;
                height = 5;
                Console.WriteLine("Detect lower 5 value is specified as MeiroGenerator arguments.");
                Console.WriteLine("Arguments width and hegiht forcibly change to 5.");
            }
            if(width % 2 == 0)
            {
                width++;
                Console.WriteLine("width must be odd integer.");
                Console.WriteLine("width forcibly increament. Results to {0}", width);
            }
            if(height % 2 == 0)
            {
                height++;
                Console.WriteLine("height must be odd integer.");
                Console.WriteLine("height forcibly increament. Results to {0}", height);
            }

            this.Width = width;
            this.Height = height;
            this.Maze = new int[Width, Height];
            this.Seed = seed;
            this.random = new Random(seed);
        }

        // 迷路の作成を開始する
        public int[,] GenerateMeiro()
        {
            // 外周を道で、それ以外を壁で埋める
            for(var i=0; i<Width; i++)
            {
                for(var j=0; j<Height; j++)
                {
                    if(i == 0 || j == 0 || i == Width-1 || j == Height-1)
                        Maze[i, j] = PATH;
                    else
                        Maze[i, j] = WALL;
                }
            }
            
            Cell start;
            while(true)
            {
                var stW = random.Next(0, Width);
                if(stW%2 == 1)
                {
                    while(true)
                    {
                        var stH = random.Next(0, Height);
                        if(stH%2 == 1)
                        {
                            start = new Cell(stW, stH);
                            break;
                        }
                    }
                    break;
                }
            }

            this.Start = start;
            Dig(start);

            // 外周を壁で埋め直す
            for(var i=0; i<Width; i++)
            {
                for(var j=0; j<Height; j++)
                {
                    if(i == 0 || j == 0 || i == Width-1 || j == Height-1)
                        Maze[i, j] = WALL;
                }
            }

            return Maze;
        }

        public int Get(Cell cell)
        {
            if(cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height)
                return Maze[cell.X, cell.Y];
            else
                return -1;
        }

        public bool Dig(Cell digFrom)
        {
            // 2マス先まで掘ることのできる方角のリスト
            var directions = new List<Direction>();
            //
            if(Get(digFrom.To(Direction.UP, 1)) == WALL && Get(digFrom.To(Direction.UP, 2)) == WALL)
                directions.Add(Direction.UP);
            if(Get(digFrom.To(Direction.DOWN, 1)) == WALL && Get(digFrom.To(Direction.DOWN, 2)) == WALL)
                directions.Add(Direction.DOWN);
            if(Get(digFrom.To(Direction.RIGHT, 1)) == WALL && Get(digFrom.To(Direction.RIGHT, 2)) == WALL)
                directions.Add(Direction.RIGHT);
            if(Get(digFrom.To(Direction.LEFT, 1)) == WALL && Get(digFrom.To(Direction.LEFT, 2)) == WALL)
                directions.Add(Direction.LEFT);

            if(directions.Count == 0)
                return false;
            
            SetPath(digFrom);

            var direction = directions[random.Next(0, directions.Count)];
            SetPath(digFrom.To(direction, 1));
            SetPath(digFrom.To(direction, 2));

            var newDigFrom = digFrom.To(direction, 2);
            if(Dig(newDigFrom))
            {
                newDigFrom = GetRandomStartingCell();
                Dig(newDigFrom);
            }
            return true;
        }

        public void SetPath(Cell cell)
        {
            if(Get(cell) != PATH)
            {
                Maze[cell.X, cell.Y] = PATH;
                Paths.Add(cell);
            }
        }

        public Cell GetRandomStartingCell()
        {
            while(true)
            {
                var cell = Paths[random.Next(0, Paths.Count)];
                if(cell.X%2 == 1 && cell.Y%2 == 1)
                    return cell;
            }
        }

        public string ExpressAsString()
        {
            var builder = new StringBuilder();
            for(var i=0; i<Width; i++)
            {
                for(var j=0; j<Height; j++)
                {
                    if(Get(new Cell(i, j)) == PATH)
                        builder.Append(" ");
                    if(Get(new Cell(i, j)) == WALL)
                        builder.Append("\u2588");
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }
    }

    // 迷路のマス
    public class Cell
    {
        public int X, Y;

        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Cell(int[] cell)
        {
            this.X = cell[0];
            this.Y = cell[1];
        }

        public Cell To(Meiro.Direction direction, int mass)
        {
            Cell cell;
            switch(direction)
            {
                case Meiro.Direction.UP:    cell = new Cell(0, 1); break;
                case Meiro.Direction.DOWN:  cell = new Cell(0, -1); break;
                case Meiro.Direction.RIGHT: cell = new Cell(1, 0); break;
                case Meiro.Direction.LEFT:  cell = new Cell(-1, 0); break;
                default:                    cell = new Cell(0, 0); break;
            }
            return this + (cell*mass);
        }

        public static Cell operator+ (Cell a, Cell b)
        {
            return new Cell(a.X+b.X, a.Y+b.Y);
        }

        public static Cell operator- (Cell a, Cell b)
        {
            return new Cell(a.X-b.X, a.Y-b.Y);
        }

        public static Cell operator* (Cell a, double b)
        {
            return new Cell((int)(a.X*b), (int)(a.Y*b));
        }
    }
}