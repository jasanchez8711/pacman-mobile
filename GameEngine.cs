namespace PacmanMobile;

public enum Direction { None, Left, Right, Up, Down }

public sealed class GameEngine
{
    public static readonly string[] OriginalMap =
    [
        "###################",
        "#o.......#.......o#",
        "#.###.##.#.##.###.#",
        "#.....#.....#.....#",
        "###.#.#.###.#.#.###",
        "#...#...#.#...#...#",
        "#.#####.#.#.#####.#",
        "#.......G.G.......#",
        "#.#####.#.#.#####.#",
        "#...#...#.#...#...#",
        "###.#.#.###.#.#.###",
        "#.....#.....#.....#",
        "#.###.##.#.##.###.#",
        "#o.......P.......o#",
        "###################"
    ];

    private static readonly Direction[] Directions =
        [Direction.Left, Direction.Right, Direction.Up, Direction.Down];

    private readonly Random random = new();
    private readonly List<Ghost> ghosts = [];
    private char[,] map = new char[1, 1];
    private Player player = new(1, 1);
    private int pellets;
    private int frightenedSteps;

    public int Columns => OriginalMap[0].Length;
    public int Rows => OriginalMap.Length;
    public int Score { get; private set; }
    public int Lives { get; private set; }
    public bool Running { get; private set; }
    public bool Paused { get; private set; }
    public bool Won { get; private set; }
    public bool GameOver { get; private set; }
    public bool Finished => Won || GameOver;
    public bool Frightened => frightenedSteps > 0;
    public float AnimationPhase { get; set; }
    public Player Player => player;
    public IReadOnlyList<Ghost> Ghosts => ghosts;

    public GameEngine() => Reset();

    public char TileAt(int x, int y) => map[y, x];

    public void Reset()
    {
        map = new char[Rows, Columns];
        ghosts.Clear();
        pellets = 0;

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                char tile = OriginalMap[y][x];
                if (tile == 'P')
                {
                    player = new Player(x, y);
                    map[y, x] = ' ';
                }
                else if (tile == 'G')
                {
                    ghosts.Add(new Ghost(x, y));
                    map[y, x] = ' ';
                }
                else
                {
                    map[y, x] = tile;
                    if (tile is '.' or 'o') pellets++;
                }
            }
        }

        Score = 0;
        Lives = 3;
        frightenedSteps = 0;
        Running = false;
        Paused = false;
        Won = false;
        GameOver = false;
    }

    public void Start()
    {
        if (Finished) Reset();
        Paused = false;
        Running = true;
    }

    public void TogglePause()
    {
        if (Finished) return;
        Paused = !Paused;
        Running = !Paused;
    }

    public void SetDirection(Direction direction) => player.Wanted = direction;

    public void Step()
    {
        if (!Running) return;
        MovePlayer();
        CheckCollisions();
        if (!Running) return;

        foreach (Ghost ghost in ghosts) MoveGhost(ghost);
        CheckCollisions();
        if (frightenedSteps > 0) frightenedSteps--;
    }

    private void MovePlayer()
    {
        if (CanMove(player.X, player.Y, player.Wanted)) player.Facing = player.Wanted;
        if (CanMove(player.X, player.Y, player.Facing))
        {
            GridDelta delta = Delta(player.Facing);
            player.X += delta.X;
            player.Y += delta.Y;
        }

        char tile = map[player.Y, player.X];
        if (tile is not ('.' or 'o')) return;
        Score += tile == 'o' ? 50 : 10;
        pellets--;
        map[player.Y, player.X] = ' ';
        if (tile == 'o') frightenedSteps = 35;

        if (pellets == 0)
        {
            Won = true;
            Running = false;
        }
    }

    private void MoveGhost(Ghost ghost)
    {
        List<Direction> options = Directions
            .Where(direction => CanMove(ghost.X, ghost.Y, direction) && direction != Opposite(ghost.Facing))
            .OrderBy(direction =>
            {
                GridDelta next = Delta(direction);
                int distance = Math.Abs(ghost.X + next.X - player.X) + Math.Abs(ghost.Y + next.Y - player.Y);
                return Frightened ? -distance : distance;
            })
            .ToList();

        if (options.Count == 0) options.Add(Opposite(ghost.Facing));
        ghost.Facing = options.Count > 1 && random.Next(5) == 0 ? options[1] : options[0];
        GridDelta delta = Delta(ghost.Facing);
        ghost.X += delta.X;
        ghost.Y += delta.Y;
    }

    private void CheckCollisions()
    {
        foreach (Ghost ghost in ghosts)
        {
            if (ghost.X != player.X || ghost.Y != player.Y) continue;
            if (Frightened)
            {
                Score += 200;
                ghost.Reset();
            }
            else
            {
                Lives--;
                if (Lives <= 0)
                {
                    GameOver = true;
                    Running = false;
                }
                else
                {
                    ResetPositions();
                }
                return;
            }
        }
    }

    private void ResetPositions()
    {
        for (int y = 0; y < Rows; y++)
        {
            int x = OriginalMap[y].IndexOf('P');
            if (x < 0) continue;
            player.X = x;
            player.Y = y;
            break;
        }
        player.Facing = Direction.Left;
        player.Wanted = Direction.Left;
        foreach (Ghost ghost in ghosts) ghost.Reset();
    }

    private bool CanMove(int x, int y, Direction direction)
    {
        GridDelta delta = Delta(direction);
        int nextX = x + delta.X;
        int nextY = y + delta.Y;
        return nextX >= 0 && nextX < Columns && nextY >= 0 && nextY < Rows && map[nextY, nextX] != '#';
    }

    private static GridDelta Delta(Direction direction) => direction switch
    {
        Direction.Left => new GridDelta(-1, 0),
        Direction.Right => new GridDelta(1, 0),
        Direction.Up => new GridDelta(0, -1),
        Direction.Down => new GridDelta(0, 1),
        _ => new GridDelta(0, 0)
    };

    private static Direction Opposite(Direction direction) => direction switch
    {
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        _ => Direction.None
    };

    private readonly record struct GridDelta(int X, int Y);
}

public sealed class Player(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public Direction Facing { get; set; } = Direction.Left;
    public Direction Wanted { get; set; } = Direction.Left;
}

public sealed class Ghost(int x, int y)
{
    private readonly int homeX = x;
    private readonly int homeY = y;
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public Direction Facing { get; set; } = Direction.Left;

    public void Reset()
    {
        X = homeX;
        Y = homeY;
        Facing = Direction.Left;
    }
}
