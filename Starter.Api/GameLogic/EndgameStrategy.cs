namespace Starter.Api.GameLogic;

/// <summary>
/// Handles endgame scenarios when the snake is large and there's only one opponent left.
/// Strategy: Aggressively minimize opponent space while maintaining survival room.
/// </summary>
public class EndgameStrategy
{
    private const double ENDGAME_SIZE_THRESHOLD = 1.87;

    /// <summary>
    /// Determines if we're in endgame mode.
    /// Endgame = snake is 1.87x grid area (width × height) AND only 1 opponent remains.
    /// </summary>
    public static bool IsEndgame(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        var opponents = allSnakes.Where(s => s.Id != you.Id).ToList();

        // Must have exactly 1 opponent
        if (opponents.Count != 1)
        {
            return false;
        }

        // Calculate grid area (total cells on board)
        double sizeRatio = (double)you.Length / board.Height;

        bool isEndgame = sizeRatio >= ENDGAME_SIZE_THRESHOLD;

        if (isEndgame)
        {
            Console.WriteLine($"=== ENDGAME MODE ACTIVATED ===");
            Console.WriteLine($"  Opponent: {opponents[0].Name} (length: {opponents[0].Length})");
        }

        return isEndgame;
    }

    /// <summary>
    /// Gets the best move for endgame: minimize opponent space while keeping yourself alive.
    /// </summary>
    public static string? GetEndgameMove(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        if (safeMoves.Count == 0)
        {
            return null;
        }

        var opponent = allSnakes.FirstOrDefault(s => s.Id != you.Id);
        if (opponent == null)
        {
            return null;
        }

        string bestMove = safeMoves[0];
        int bestScore = int.MinValue;

        Console.WriteLine("Evaluating endgame moves:");

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(you.Head, move);

            // Calculate endgame score for this move
            int score = CalculateEndgameScore(nextPos, board, you, opponent, allSnakes);

            Console.WriteLine($"  Move {move} -> ({nextPos.X},{nextPos.Y}): Score={score}");

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Console.WriteLine($"=== ENDGAME MOVE: {bestMove} (score: {bestScore}) ===");
        return bestMove;
    }

    /// <summary>
    /// Calculates endgame score: heavily weights minimizing opponent space.
    /// Formula: (YourSpace - OpponentSpace * 3) + ProximityBonus
    /// </summary>
    private static int CalculateEndgameScore(
        Coordinate position,
        Board board,
        Snake you,
        Snake opponent,
        IEnumerable<Snake> allSnakes)
    {
        // Calculate reachable space for both snakes
        int yourSpace = CalculateReachableSpace(position, board, you, allSnakes);
        int opponentSpace = CalculateReachableSpace(opponent.Head, board, opponent, allSnakes);

        // Ensure we have enough space to survive (at least our body length)
        if (yourSpace < you.Length)
        {
            // Penalty for moves that trap us
            return int.MinValue + yourSpace;
        }

        // Calculate distance to opponent (for positioning)
        int distanceToOpponent = Math.Abs(position.X - opponent.Head.X) + Math.Abs(position.Y - opponent.Head.Y);

        // Endgame scoring:
        // 1. Maximize your space (weight: 1x)
        // 2. AGGRESSIVELY minimize opponent space (weight: 3x)
        // 3. Bonus for getting closer to opponent to apply pressure (weight: -2 per square)
        int spaceScore = yourSpace - (opponentSpace * 3);
        int proximityBonus = -2 * distanceToOpponent; // Negative distance = bonus for being closer

        int totalScore = spaceScore + proximityBonus;

        return totalScore;
    }

    /// <summary>
    /// Calculates reachable space using flood fill.
    /// </summary>
    private static int CalculateReachableSpace(
        Coordinate start,
        Board board,
        Snake snake,
        IEnumerable<Snake> allSnakes)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(start);
        visited.Add($"{start.X},{start.Y}");

        int count = 0;
        int maxCells = board.Width * board.Height;

        while (queue.Count > 0 && count < maxCells)
        {
            var current = queue.Dequeue();
            count++;

            var neighbors = new List<Coordinate>
            {
                new(current.X, current.Y + 1),
                new(current.X, current.Y - 1),
                new(current.X - 1, current.Y),
                new(current.X + 1, current.Y)
            };

            foreach (var neighbor in neighbors)
            {
                var key = $"{neighbor.X},{neighbor.Y}";
                if (!visited.Contains(key) && IsAccessible(neighbor, board, snake, allSnakes))
                {
                    visited.Add(key);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Checks if a position is accessible (more lenient than IsSafeMove).
    /// </summary>
    private static bool IsAccessible(
        Coordinate position,
        Board board,
        Snake snake,
        IEnumerable<Snake> allSnakes)
    {
        // Check boundaries
        if (position.X < 0 || position.X >= board.Width ||
            position.Y < 0 || position.Y >= board.Height)
        {
            return false;
        }

        // Check own body (skip tail since it moves)
        var yourBody = snake.Body.ToList();
        for (int i = 0; i < yourBody.Count - 1; i++)
        {
            if (yourBody[i].X == position.X && yourBody[i].Y == position.Y)
            {
                return false;
            }
        }

        // Check other snakes' bodies (skip tails)
        foreach (var otherSnake in allSnakes.Where(s => s.Id != snake.Id))
        {
            var otherBody = otherSnake.Body.ToList();
            for (int i = 0; i < otherBody.Count - 1; i++)
            {
                if (otherBody[i].X == position.X && otherBody[i].Y == position.Y)
                {
                    return false;
                }
            }
        }

        // Check hazards
        if (board.Hazards != null)
        {
            foreach (var hazard in board.Hazards)
            {
                if (hazard.X == position.X && hazard.Y == position.Y)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
