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

        // Step 2: Critical health check - prioritize food when health < 50
        if (FoodTargeting.ShouldSeekFood(you))
        {
            var foodMove = FoodTargeting.GetMoveTowardsFood(board, you, allSnakes, safeMoves);
            if (foodMove != null)
            {
                Console.WriteLine($"Health critical ({you.Health}), seeking food: {foodMove}");
                return foodMove;
            }
        }

        // Step 3: If healthy, consider targeting smaller opponents
        if (OpponentTargeting.ShouldTargetOpponents(you, allSnakes))
        {
            var opponentMove = OpponentTargeting.GetMoveTowardsOpponent(board, you, allSnakes, safeMoves);
            if (opponentMove != null)
            {
                Console.WriteLine($"Targeting smaller opponent: {opponentMove}");
                return opponentMove;
            }
        }

        // Step 4: Default - seek food to maintain health
        var defaultFoodMove = FoodTargeting.GetMoveTowardsFood(board, you, allSnakes, safeMoves);
        if (defaultFoodMove != null)
        {
            Console.WriteLine($"Seeking food to maintain health: {defaultFoodMove}");
            return defaultFoodMove;
        }

        // Step 5: If no targets, prefer moves that maximize space (flood fill heuristic)
        var bestMove = GetMoveWithMostSpace(board, you, allSnakes, safeMoves);
        Console.WriteLine($"No targets found, maximizing space: {bestMove}");
        return bestMove;
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
        // Simple flood fill to count accessible spaces
        var visited = new HashSet<string>();
        var queue = new Queue<Coordinate>();
        queue.Enqueue(position);
        visited.Add($"{position.X},{position.Y}");

        int count = 0;
        int maxDepth = 10; // Limit search depth for performance

        while (queue.Count > 0 && count < maxDepth)
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
