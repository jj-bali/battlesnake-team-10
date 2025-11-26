namespace Starter.Api.Logic;

/// <summary>
/// Validates moves and checks for safety
/// </summary>
public class MoveValidator
{
    /// <summary>
    /// Gets all possible moves (up, down, left, right)
    /// </summary>
    public static List<string> GetAllMoves()
    {
        return new List<string> { "up", "down", "left", "right" };
    }

    /// <summary>
    /// Converts a direction string to a coordinate offset
    /// </summary>
    public static Coordinate GetDirectionOffset(string direction)
    {
        return direction switch
        {
            "up" => new Coordinate(0, 1),
            "down" => new Coordinate(0, -1),
            "left" => new Coordinate(-1, 0),
            "right" => new Coordinate(1, 0),
            _ => new Coordinate(0, 0)
        };
    }

    /// <summary>
    /// Gets the coordinate that results from a move
    /// </summary>
    public static Coordinate GetNewPosition(Coordinate head, string direction)
    {
        var offset = GetDirectionOffset(direction);
        return new Coordinate(head.X + offset.X, head.Y + offset.Y);
    }

    /// <summary>
    /// Filters out unsafe moves
    /// </summary>
    public static List<string> GetSafeMoves(Board board, Snake you)
    {
        var allMoves = GetAllMoves();
        var safeMoves = new List<string>();

        foreach (var move in allMoves)
        {
            var newPosition = GetNewPosition(you.Head, move);

            if (IsMoveValid(newPosition, board, you))
            {
                safeMoves.Add(move);
            }
        }

        return safeMoves;
    }

    /// <summary>
    /// Checks if a move is valid (doesn't result in immediate death)
    /// </summary>
    public static bool IsMoveValid(Coordinate position, Board board, Snake you)
    {
        // Check 1: Out of bounds
        if (!IsInBounds(position, board))
            return false;

        // Check 2: Self-collision
        if (IsCollidingWithSelf(position, you))
            return false;

        // Check 3: Collision with other snakes
        if (IsCollidingWithOtherSnakes(position, board, you))
            return false;

        // Check 4: Head-to-head collision with larger or equal snakes
        if (IsDangerousHeadToHead(position, board, you))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a position is within the board bounds
    /// </summary>
    public static bool IsInBounds(Coordinate position, Board board)
    {
        return position.X >= 0 && position.X < board.Width &&
               position.Y >= 0 && position.Y < board.Height;
    }

    /// <summary>
    /// Checks if a position collides with your own body
    /// </summary>
    public static bool IsCollidingWithSelf(Coordinate position, Snake you)
    {
        // Check all body segments except the tail (tail will move)
        var bodySegments = you.Body.ToList();
        for (int i = 0; i < bodySegments.Count - 1; i++)
        {
            if (bodySegments[i].X == position.X && bodySegments[i].Y == position.Y)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a position collides with other snakes' bodies
    /// </summary>
    public static bool IsCollidingWithOtherSnakes(Coordinate position, Board board, Snake you)
    {
        foreach (var snake in board.Snakes)
        {
            if (snake.Id == you.Id)
                continue;

            // Check body segments (excluding tail)
            var bodySegments = snake.Body.ToList();
            for (int i = 0; i < bodySegments.Count - 1; i++)
            {
                if (bodySegments[i].X == position.X && bodySegments[i].Y == position.Y)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a position could result in a dangerous head-to-head collision
    /// </summary>
    public static bool IsDangerousHeadToHead(Coordinate position, Board board, Snake you)
    {
        foreach (var snake in board.Snakes)
        {
            if (snake.Id == you.Id)
                continue;

            // Get all possible moves for the opponent
            var opponentMoves = GetAllMoves();
            foreach (var opponentMove in opponentMoves)
            {
                var opponentNewPos = GetNewPosition(snake.Head, opponentMove);

                // If opponent could move to the same position and they're >= our size, it's dangerous
                if (opponentNewPos.X == position.X && opponentNewPos.Y == position.Y &&
                    snake.Length >= you.Length)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluates space available from a position using flood fill
    /// </summary>
    public static int EvaluateSpace(Coordinate position, Board board, Snake you)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(position);
        visited.Add($"{position.X},{position.Y}");

        int spaceCount = 0;
        int maxDepth = Math.Max(board.Width, board.Height);

        while (queue.Count > 0 && spaceCount < maxDepth * maxDepth)
        {
            var current = queue.Dequeue();
            spaceCount++;

            var neighbors = new[]
            {
                new Coordinate(current.X + 1, current.Y),
                new Coordinate(current.X - 1, current.Y),
                new Coordinate(current.X, current.Y + 1),
                new Coordinate(current.X, current.Y - 1)
            };

            foreach (var neighbor in neighbors)
            {
                var key = $"{neighbor.X},{neighbor.Y}";
                if (visited.Contains(key))
                    continue;

                if (IsInBounds(neighbor, board) &&
                    !IsCollidingWithSelf(neighbor, you) &&
                    !IsCollidingWithOtherSnakes(neighbor, board, you))
                {
                    visited.Add(key);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return spaceCount;
    }
}