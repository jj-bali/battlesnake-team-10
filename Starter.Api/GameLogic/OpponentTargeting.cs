using Starter.Api.Pathfinding;

namespace Starter.Api.GameLogic;

public class OpponentTargeting
{
    public static Snake? FindNearestSmallerSnake(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        var otherSnakes = allSnakes.Where(s => s.Id != you.Id && s.Length < you.Length).ToList();

        if (!otherSnakes.Any())
        {
            return null;
        }

        Snake? nearestSmaller = null;
        int shortestDistance = int.MaxValue;

        foreach (var snake in otherSnakes)
        {
            var distance = ManhattanDistance(you.Head, snake.Head);

            if (distance < shortestDistance)
            {
                // Verify there's a path to this snake
                var path = AStarPathfinder.FindPath(you.Head, snake.Head, board, you, allSnakes);
                if (path != null && path.Count > 0)
                {
                    nearestSmaller = snake;
                    shortestDistance = distance;
                }
            }
        }

        return nearestSmaller;
    }

    public static string? GetMoveTowardsOpponent(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        var targetSnake = FindNearestSmallerSnake(board, you, allSnakes);

        if (targetSnake == null)
        {
            return null;
        }

        // Use A* to find path to opponent's head
        var path = AStarPathfinder.FindPath(you.Head, targetSnake.Head, board, you, allSnakes);

        if (path == null || path.Count < 2)
        {
            return null;
        }

        // Get the first move in the path
        var nextPosition = path[1];
        var direction = MoveValidator.GetDirectionFromCoordinates(you.Head, nextPosition);

        // Only return if it's a safe move
        return safeMoves.Contains(direction) ? direction : null;
    }

    public static bool ShouldTargetOpponents(Snake you, IEnumerable<Snake> allSnakes)
    {
        // Only be aggressive if we're healthy (above 50 health) and there are smaller snakes
        return you.Health >= 50 && allSnakes.Any(s => s.Id != you.Id && s.Length < you.Length);
    }

    private static int ManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
