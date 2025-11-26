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
                    // NEW: Check if we can survive AFTER eating this food
                    if (IsFoodSafeToEat(food, board, you, allSnakes))
                    {
                        nearestFood = food;
                        shortestDistance = distance;
                    }
                }
            }
        }

        return nearestFood;
    }

    public static bool ShouldSeekFood(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Critical: Always seek food when health is low
        if (you.Health < 50)
        {
            return true;
        }

        // Check size strategy
        var sizeStrategy = SizeOptimizer.GetSizeStrategy(board, you, allSnakes);

        // If we're too big, avoid food unless health demands it
        if (sizeStrategy.Mode == GrowthMode.Avoid)
        {
            return false; // Skip food to maintain mobility
        }

        // If we're smaller than optimal or need to grow, seek food
        if (sizeStrategy.Mode == GrowthMode.Aggressive || sizeStrategy.Mode == GrowthMode.Moderate)
        {
            return true;
        }

        // Maintain mode: Only seek food when health gets somewhat low
        return you.Health < 70;
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
            return null;
        }

        // Use A* to find the best path
        var path = AStarPathfinder.FindPath(you.Head, nearestFood, board, you, allSnakes);

        if (path == null || path.Count < 2)
        {
            return null;
        }

        // Get the first move in the path
        var nextPosition = path[1]; // path[0] is current position
        var direction = MoveValidator.GetDirectionFromCoordinates(you.Head, nextPosition);

        // Only return if it's a safe move
        return safeMoves.Contains(direction) ? direction : null;
    }

    private static int ManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static bool IsFoodSafeToEat(Coordinate foodPosition, Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Simulate eating the food - check if we have escape routes afterward
        // When we eat food, our head moves to food position and tail doesn't move (we grow)

        // Create a simulated snake state after eating food
        var bodyList = you.Body.ToList();
        var simulatedBody = new List<Coordinate> { foodPosition }; // Head at food position
        simulatedBody.AddRange(bodyList); // Add all existing body segments (tail doesn't move when eating)

        // Check available moves from the food position
        var movesFromFood = new List<string> { "up", "down", "left", "right" };
        int safeMovesCount = 0;
        int spaciousMovesCount = 0;

        foreach (var move in movesFromFood)
        {
            var nextPos = MoveValidator.GetNextPosition(foodPosition, move);

            // Check if this move would be safe
            if (!IsWithinBounds(nextPos, board))
                continue;

            // Check collision with our new body (after eating)
            bool wouldCollideWithSelf = false;
            // Check all segments except tail (which will move on next turn)
            for (int i = 0; i < simulatedBody.Count - 1; i++)
            {
                if (simulatedBody[i].X == nextPos.X && simulatedBody[i].Y == nextPos.Y)
                {
                    wouldCollideWithSelf = true;
                    break;
                }
            }

            if (wouldCollideWithSelf)
                continue;

            // Check collision with other snakes
            bool wouldCollideWithOthers = false;
            foreach (var snake in allSnakes.Where(s => s.Id != you.Id))
            {
                foreach (var segment in snake.Body)
                {
                    if (segment.X == nextPos.X && segment.Y == nextPos.Y)
                    {
                        wouldCollideWithOthers = true;
                        break;
                    }
                }
                if (wouldCollideWithOthers) break;
            }

            if (wouldCollideWithOthers)
                continue;

            // This move is safe
            safeMovesCount++;

            // Check if this move has enough space (flood fill from next position)
            var simulatedSnake = new Snake
            {
                Id = you.Id,
                Head = foodPosition,
                Body = simulatedBody,
                Length = you.Length + 1, // We grew by eating
                Health = 100 // Restored after eating
            };

            var availableSpace = CalculateSpaceFromPosition(nextPos, board, simulatedSnake, allSnakes);

            // We want at least enough space for our body length
            if (availableSpace >= simulatedSnake.Length)
            {
                spaciousMovesCount++;
            }
        }

        // Critical: If health is very low (< 30), we MUST eat even if somewhat risky
        if (you.Health < 30)
        {
            // At least need ONE safe move to survive
            return safeMovesCount > 0;
        }

        // If health is okay, be more conservative
        // Require at least 2 safe moves OR 1 spacious move
        return spaciousMovesCount > 0 || safeMovesCount >= 2;
    }

    private static bool IsWithinBounds(Coordinate position, Board board)
    {
        return position.X >= 0 && position.X < board.Width &&
               position.Y >= 0 && position.Y < board.Height;
    }

    private static int CalculateSpaceFromPosition(Coordinate position, Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Simple flood fill to count accessible spaces
        var visited = new HashSet<string>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(position);
        visited.Add($"{position.X},{position.Y}");

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
                if (!visited.Contains(key) && MoveValidator.IsSafeMove(neighbor, board, you, allSnakes))
                {
                    visited.Add(key);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return count;
    }
}
