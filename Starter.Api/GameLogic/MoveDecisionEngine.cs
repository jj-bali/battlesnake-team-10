namespace Starter.Api.GameLogic;

public class MoveDecisionEngine
{
    public static string DecideMove(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Step 1: Get all safe moves (never move into danger)
        var safeMoves = MoveValidator.GetSafeMoves(board, you, allSnakes);

        if (safeMoves.Count == 0)
        {
            // No safe moves - find the least dangerous move
            Console.WriteLine("WARNING: No safe moves available! Choosing least dangerous move.");

            var allMoves = new List<string> { "up", "down", "left", "right" };

            // Priority 1: Moves that are in bounds and don't hit our own body
            var nonSelfCollisionMoves = new List<string>();

            foreach (var move in allMoves)
            {
                var nextPos = MoveValidator.GetNextPosition(you.Head, move);

                // Check if in bounds
                if (nextPos.X < 0 || nextPos.X >= board.Width ||
                    nextPos.Y < 0 || nextPos.Y >= board.Height)
                {
                    continue; // Skip out of bounds moves
                }

                // Check if it would hit our own body
                bool hitsOwnBody = false;
                foreach (var segment in you.Body)
                {
                    if (segment.X == nextPos.X && segment.Y == nextPos.Y)
                    {
                        hitsOwnBody = true;
                        break;
                    }
                }

                if (!hitsOwnBody)
                {
                    nonSelfCollisionMoves.Add(move);
                }
            }

            if (nonSelfCollisionMoves.Count > 0)
            {
                Console.WriteLine($"Choosing move that avoids self-collision: {nonSelfCollisionMoves[0]}");
                return nonSelfCollisionMoves[Random.Shared.Next(nonSelfCollisionMoves.Count)];
            }

            // Priority 2: Just stay in bounds (desperate situation)
            var inBoundsMoves = new List<string>();
            foreach (var move in allMoves)
            {
                var nextPos = MoveValidator.GetNextPosition(you.Head, move);
                if (nextPos.X >= 0 && nextPos.X < board.Width &&
                    nextPos.Y >= 0 && nextPos.Y < board.Height)
                {
                    inBoundsMoves.Add(move);
                }
            }

            if (inBoundsMoves.Count > 0)
            {
                Console.WriteLine("No moves avoid collision, choosing in-bounds move");
                return inBoundsMoves[Random.Shared.Next(inBoundsMoves.Count)];
            }

            // This should never happen, but failsafe
            Console.WriteLine("All moves go out of bounds - choosing random move as last resort");
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

        // Step 4: If no targets, prefer moves that maximize space (prioritize survival)
        var bestMove = GetMoveWithMostSpace(board, you, allSnakes, safeMoves);
        Console.WriteLine($"Prioritizing survival, maximizing space: {bestMove}");
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
