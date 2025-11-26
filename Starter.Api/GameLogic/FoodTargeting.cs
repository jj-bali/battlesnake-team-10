using Starter.Api.Pathfinding;

namespace Starter.Api.GameLogic;

public class FoodTargeting
{
    public static Coordinate? FindNearestFood(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        if (board.Food == null || !board.Food.Any())
        {
            return null;
        }

        Coordinate? nearestFood = null;
        int shortestDistance = int.MaxValue;

        foreach (var food in board.Food)
        {
            var distance = ManhattanDistance(you.Head, food);

            // Prefer closer food
            if (distance < shortestDistance)
            {
                // Verify there's a path to this food
                var path = AStarPathfinder.FindPath(you.Head, food, board, you, allSnakes);
                if (path != null && path.Count > 0)
                {
                    nearestFood = food;
                    shortestDistance = distance;
                }
            }
        }

        return nearestFood;
    }

    public static bool ShouldSeekFood(Snake you)
    {
        // Prioritize food when health is critically low
        var res = you.Health < 5;
        Console.WriteLine($"Health is critically low: {res}");
        Console.WriteLine($"Current health is: {you.Health}");
        return you.Health < 5;
    }

    public static string? GetMoveTowardsFood(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        var nearestFood = FindNearestFood(board, you, allSnakes);

        if (nearestFood == null)
        {
            Console.WriteLine("The nearestFood is null");
            return null;
        }

        // Use A* to find the best path
        var path = AStarPathfinder.FindPath(you.Head, nearestFood, board, you, allSnakes);

        if (path == null || path.Count < 2)
        {
            Console.WriteLine("A* could not find a path!");
            return null;
        }

        // Get the first move in the path
        var nextPosition = path[1]; // path[0] is current position
        var direction = MoveValidator.GetDirectionFromCoordinates(you.Head, nextPosition);

        // Only return if it's a safe move
        Console.WriteLine("Will return a Safe Move");
        return safeMoves.Contains(direction) ? direction : null;
    }

    private static int ManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
