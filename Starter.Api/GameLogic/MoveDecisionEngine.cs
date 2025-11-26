namespace Starter.Api.GameLogic;

public class MoveDecisionEngine
{
    public static string DecideMove(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Step 1: Get all safe moves (never move into danger)
        var safeMoves = MoveValidator.GetSafeMoves(board, you, allSnakes);

        if (safeMoves.Count == 0)
        {
            // No safe moves - pick any move as last resort
            Console.WriteLine("WARNING: No safe moves available! Choosing random move.");
            var allMoves = new List<string> { "up", "down", "left", "right" };
            return allMoves[Random.Shared.Next(allMoves.Count)];
        }

        // Step 2: Calculate size strategy (used throughout decision making)
        var sizeStrategy = SizeOptimizer.GetSizeStrategy(board, you, allSnakes);

        // Step 3: Filter out moves that lead to dead ends (trap avoidance)
        var spaciousMoves = FilterMovesWithMinimumSpace(board, you, allSnakes, safeMoves);

        // If all moves lead to traps, we have to pick the least bad option
        var movesToConsider = spaciousMoves.Count > 0 ? spaciousMoves : safeMoves;

        // Step 4: Check if we should seek food (considers health AND optimal size)
        if (FoodTargeting.ShouldSeekFood(board, you, allSnakes))
        {
            var foodMove = FoodTargeting.GetMoveTowardsFood(board, you, allSnakes, movesToConsider);
            if (foodMove != null)
            {
                Console.WriteLine($"Seeking food: Health={you.Health}, Size={you.Length}/{sizeStrategy.OptimalSize}, Mode={sizeStrategy.Mode}");
                return foodMove;
            }
        }

        // Step 5: If healthy, consider targeting smaller opponents
        if (OpponentTargeting.ShouldTargetOpponents(you, allSnakes))
        {
            var opponentMove = OpponentTargeting.GetMoveTowardsOpponent(board, you, allSnakes, movesToConsider);
            if (opponentMove != null)
            {
                Console.WriteLine($"Targeting smaller opponent: {opponentMove}");
                return opponentMove;
            }
        }

        // Step 6: Default - seek food to maintain health (if size allows)
        if (sizeStrategy.Mode != GrowthMode.Avoid)
        {
            var defaultFoodMove = FoodTargeting.GetMoveTowardsFood(board, you, allSnakes, movesToConsider);
            if (defaultFoodMove != null)
            {
                Console.WriteLine($"Seeking food to maintain health: {defaultFoodMove}");
                return defaultFoodMove;
            }
        }

        // Step 7: If no targets, prefer moves that maximize space (flood fill heuristic)
        var bestMove = GetMoveWithMostSpace(board, you, allSnakes, movesToConsider);
        Console.WriteLine($"No targets found, maximizing space: {bestMove}");
        return bestMove;
    }

    private static List<string> FilterMovesWithMinimumSpace(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        // Calculate minimum space needed to avoid being trapped
        // We need at least as much space as our body length to safely navigate
        int minRequiredSpace = you.Length;

        var spaciousMoves = new List<string>();

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(you.Head, move);
            var availableSpace = CalculateAvailableSpace(nextPos, board, you, allSnakes);

            // Only consider moves that give us enough room to maneuver
            if (availableSpace >= minRequiredSpace)
            {
                spaciousMoves.Add(move);
            }
        }

        return spaciousMoves;
    }

    private static string GetMoveWithMostSpace(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        string bestMove = safeMoves[0];
        int maxSpace = 0;

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(you.Head, move);
            var space = CalculateAvailableSpace(nextPos, board, you, allSnakes);

            if (space > maxSpace)
            {
                maxSpace = space;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private static int CalculateAvailableSpace(
        Coordinate position,
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes)
    {
        // Flood fill to count accessible spaces from this position
        var visited = new HashSet<string>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(position);
        visited.Add($"{position.X},{position.Y}");

        int count = 0;
        // Limit based on board size to avoid infinite loops, but allow thorough search
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
