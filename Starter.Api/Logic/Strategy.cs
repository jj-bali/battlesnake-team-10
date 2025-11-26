namespace Starter.Api.Logic;

/// <summary>
/// Strategic decision-making for the Battlesnake
/// </summary>
public class Strategy
{
    private const int LowHealthThreshold = 15;

    /// <summary>
    /// Finds the nearest food item
    /// </summary>
    public static Coordinate? FindNearestFood(Coordinate head, Board board)
    {
        if (!board.Food.Any())
            return null;

        return board.Food
            .OrderBy(food => PathFinding.CalculateManhattanDistance(head, food))
            .FirstOrDefault();
    }

    /// <summary>
    /// Finds the nearest smaller snake (for aggression/tactical positioning)
    /// </summary>
    public static Snake? FindNearestSmallerSnake(Snake you, Board board)
    {
        var smallerSnakes = board.Snakes
            .Where(s => s.Id != you.Id && s.Length < you.Length)
            .ToList();

        if (!smallerSnakes.Any())
            return null;

        return smallerSnakes
            .OrderBy(snake => PathFinding.CalculateManhattanDistance(you.Head, snake.Head))
            .FirstOrDefault();
    }

    /// <summary>
    /// Determines the best target based on health and game state
    /// </summary>
    public static Coordinate? DetermineTarget(Snake you, Board board)
    {
        // Priority 1: If health is low, go for food
        if (you.Health < LowHealthThreshold)
        {
            return FindNearestFood(you.Head, board);
        }

        // Priority 2: Target smaller snakes for aggression
        var smallerSnake = FindNearestSmallerSnake(you, board);
        if (smallerSnake != null)
        {
            return smallerSnake.Head;
        }

        // Priority 3: If no smaller snakes, go for food to grow
        var nearestFood = FindNearestFood(you.Head, board);
        if (nearestFood != null)
        {
            return nearestFood;
        }

        // Priority 4: Move to center of board if no other target
        return new Coordinate(board.Width / 2, board.Height / 2);
    }

    /// <summary>
    /// Chooses the best move based on strategy and safety
    /// </summary>
    public static string ChooseBestMove(Snake you, Board board)
    {
        // Get all safe moves
        var safeMoves = MoveValidator.GetSafeMoves(board, you);

        // If no safe moves, try any move as a last resort
        if (safeMoves.Count == 0)
        {
            var allMoves = MoveValidator.GetAllMoves();
            return allMoves[Random.Shared.Next(allMoves.Count)];
        }

        // If only one safe move, take it
        if (safeMoves.Count == 1)
        {
            return safeMoves[0];
        }

        // Determine target
        var target = DetermineTarget(you, board);

        if (target != null)
        {
            // Try to find a path to the target
            var path = PathFinding.FindPath(you.Head, target, board, you);

            if (path.Count >= 2)
            {
                // The first element is our current position, the second is the next step
                var nextStep = path[1];
                var direction = GetDirectionToCoordinate(you.Head, nextStep);

                // If the direction from the path is safe, use it
                if (safeMoves.Contains(direction))
                {
                    return direction;
                }
            }
        }

        // If no path to target or target move isn't safe, choose the safest move
        return ChooseSafestMove(safeMoves, board, you);
    }

    /// <summary>
    /// Chooses the safest move from available moves (most space available)
    /// </summary>
    private static string ChooseSafestMove(List<string> safeMoves, Board board, Snake you)
    {
        string bestMove = safeMoves[0];
        int maxSpace = 0;

        foreach (var move in safeMoves)
        {
            var newPosition = MoveValidator.GetNewPosition(you.Head, move);
            var space = MoveValidator.EvaluateSpace(newPosition, board, you);

            if (space > maxSpace)
            {
                maxSpace = space;
                bestMove = move;
            }
        }

        return bestMove;
    }

    /// <summary>
    /// Gets the direction to move from one coordinate to another (adjacent)
    /// </summary>
    private static string GetDirectionToCoordinate(Coordinate from, Coordinate to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (dx == 1) return "right";
        if (dx == -1) return "left";
        if (dy == 1) return "up";
        if (dy == -1) return "down";

        // Default fallback
        return "up";
    }

    /// <summary>
    /// Generates a contextual shout message
    /// </summary>
    public static string GenerateShout(Snake you, Board board, string move)
    {
        var shouts = new List<string>();

        if (you.Health < LowHealthThreshold)
        {
            shouts.Add("Hungry!");
            shouts.Add("Need food!");
            shouts.Add("So hungry...");
        }
        else if (you.Length > 10)
        {
            shouts.Add("I'm getting big!");
            shouts.Add("Fear me!");
            shouts.Add("Watch out!");
        }
        else
        {
            shouts.Add($"Moving {move}!");
            shouts.Add("Let's go!");
            shouts.Add("Still alive!");
        }

        return shouts[Random.Shared.Next(shouts.Count)];
    }
}