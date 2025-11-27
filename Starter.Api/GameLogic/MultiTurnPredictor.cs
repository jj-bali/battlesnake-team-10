namespace Starter.Api.GameLogic;

/// <summary>
/// Predicts future board states multiple turns ahead to evaluate strategic moves.
/// Focuses on cutting off opponent escape routes and space control.
/// </summary>
public class MultiTurnPredictor
{
    private const int DEFAULT_PREDICTION_DEPTH = 3; // Look 3 turns ahead
    private const int MAX_PREDICTION_DEPTH = 5; // Maximum lookahead for endgame

    /// <summary>
    /// Evaluates moves based on multi-turn prediction of space control.
    /// Returns best move that cuts off opponent escape routes.
    /// </summary>
    public static string? GetMoveWithBestFuturePosition(
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves,
        bool isEndgame = false)
    {
        if (safeMoves.Count == 0)
        {
            return null;
        }

        var opponent = allSnakes.FirstOrDefault(s => s.Id != you.Id);
        if (opponent == null)
        {
            // No opponent, just use regular space maximization
            return null;
        }

        int depth = isEndgame ? MAX_PREDICTION_DEPTH : DEFAULT_PREDICTION_DEPTH;

        string bestMove = safeMoves[0];
        double bestScore = double.MinValue;

        Console.WriteLine($"=== Multi-Turn Prediction (depth={depth}) ===");

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(you.Head, move);

            // Simulate future board states
            var score = PredictFutureSpaceControl(
                board, you, opponent, allSnakes, move, depth);

            Console.WriteLine($"  Move {move}: Future Score = {score:F2}");

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Console.WriteLine($"=== Best Predictive Move: {bestMove} (score: {bestScore:F2}) ===");
        return bestMove;
    }

    /// <summary>
    /// Predicts space control several turns into the future.
    /// Evaluates how well a move cuts off opponent escape routes.
    /// </summary>
    private static double PredictFutureSpaceControl(
        Board board,
        Snake you,
        Snake opponent,
        IEnumerable<Snake> allSnakes,
        string initialMove,
        int depth)
    {
        // Simulate the board state after our initial move
        var simulatedBoard = SimulateBoardState(board, you, opponent, allSnakes, initialMove);

        if (simulatedBoard == null)
        {
            // Invalid simulation (shouldn't happen with safe moves)
            return double.MinValue;
        }

        double totalScore = 0;
        double depthWeight = 1.0;

        // Evaluate each future turn
        for (int turn = 0; turn < depth; turn++)
        {
            if (simulatedBoard == null)
            {
                break;
            }

            var yourSimSnake = simulatedBoard.Value.you;
            var oppSimSnake = simulatedBoard.Value.opponent;
            var simAllSnakes = simulatedBoard.Value.allSnakes;

            // Calculate space for both snakes at this future state
            int yourSpace = CalculateReachableSpace(
                yourSimSnake.Head, board, yourSimSnake, simAllSnakes);
            int opponentSpace = CalculateReachableSpace(
                oppSimSnake.Head, board, oppSimSnake, simAllSnakes);

            // Calculate escape route reduction
            int escapeRouteReduction = CalculateEscapeRouteReduction(
                board, oppSimSnake, simAllSnakes);

            // Score formula:
            // - Maximize your space
            // - Minimize opponent space (heavily weighted)
            // - Bonus for reducing opponent escape routes
            // - Weight decreases with depth (near-term matters more)
            double turnScore =
                (yourSpace * 1.0) -
                (opponentSpace * 2.5) +
                (escapeRouteReduction * 5.0);

            totalScore += turnScore * depthWeight;

            // Reduce weight for future turns (exponential decay)
            depthWeight *= 0.7;

            // Simulate next turn (both snakes move optimally)
            simulatedBoard = SimulateNextTurn(board, yourSimSnake, oppSimSnake, simAllSnakes);
        }

        return totalScore;
    }

    /// <summary>
    /// Simulates the board state after both snakes make one move.
    /// Your snake makes the specified move, opponent makes their best move.
    /// </summary>
    private static (Snake you, Snake opponent, IEnumerable<Snake> allSnakes)? SimulateBoardState(
        Board board,
        Snake you,
        Snake opponent,
        IEnumerable<Snake> allSnakes,
        string yourMove)
    {
        // Simulate your move
        var yourNewHead = MoveValidator.GetNextPosition(you.Head, yourMove);
        var yourNewBody = new List<Coordinate> { yourNewHead };
        yourNewBody.AddRange(you.Body.Take(you.Length - 1)); // Remove tail

        // Predict opponent's best move (they try to maximize their space)
        var opponentMove = PredictOpponentMove(board, opponent, allSnakes);
        if (opponentMove == null)
        {
            // Opponent has no moves, they lose
            return null;
        }

        var oppNewHead = MoveValidator.GetNextPosition(opponent.Head, opponentMove);
        var oppNewBody = new List<Coordinate> { oppNewHead };
        oppNewBody.AddRange(opponent.Body.Take(opponent.Length - 1));

        // Create simulated snakes
        var simYou = new Snake
        {
            Id = you.Id,
            Head = yourNewHead,
            Body = yourNewBody,
            Length = you.Length,
            Health = you.Health - 1 // Health decreases each turn
        };

        var simOpponent = new Snake
        {
            Id = opponent.Id,
            Head = oppNewHead,
            Body = oppNewBody,
            Length = opponent.Length,
            Health = opponent.Health - 1
        };

        var simAllSnakes = new List<Snake> { simYou, simOpponent };

        return (simYou, simOpponent, simAllSnakes);
    }

    /// <summary>
    /// Simulates the next turn with both snakes moving optimally.
    /// </summary>
    private static (Snake you, Snake opponent, IEnumerable<Snake> allSnakes)? SimulateNextTurn(
        Board board,
        Snake you,
        Snake opponent,
        IEnumerable<Snake> allSnakes)
    {
        // Get safe moves for both snakes
        var yourSafeMoves = MoveValidator.GetSafeMoves(board, you, allSnakes);
        var oppSafeMoves = MoveValidator.GetSafeMoves(board, opponent, allSnakes);

        if (yourSafeMoves.Count == 0 || oppSafeMoves.Count == 0)
        {
            // One or both snakes die
            return null;
        }

        // Both snakes choose their best move (maximize own space)
        var yourBestMove = GetBestMoveForSpace(board, you, allSnakes, yourSafeMoves);
        var oppBestMove = GetBestMoveForSpace(board, opponent, allSnakes, oppSafeMoves);

        return SimulateBoardState(board, you, opponent, allSnakes, yourBestMove);
    }

    /// <summary>
    /// Predicts what move the opponent will make (assumes they play optimally).
    /// </summary>
    private static string? PredictOpponentMove(
        Board board,
        Snake opponent,
        IEnumerable<Snake> allSnakes)
    {
        var safeMoves = MoveValidator.GetSafeMoves(board, opponent, allSnakes);

        if (safeMoves.Count == 0)
        {
            return null;
        }

        // Assume opponent picks move with most space
        return GetBestMoveForSpace(board, opponent, allSnakes, safeMoves);
    }

    /// <summary>
    /// Gets the move that maximizes available space for a snake.
    /// </summary>
    private static string GetBestMoveForSpace(
        Board board,
        Snake snake,
        IEnumerable<Snake> allSnakes,
        List<string> safeMoves)
    {
        string bestMove = safeMoves[0];
        int maxSpace = 0;

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(snake.Head, move);
            var space = CalculateReachableSpace(nextPos, board, snake, allSnakes);

            if (space > maxSpace)
            {
                maxSpace = space;
                bestMove = move;
            }
        }

        return bestMove;
    }

    /// <summary>
    /// Calculates how many escape routes an opponent has.
    /// Fewer escape routes means they're more trapped.
    /// </summary>
    private static int CalculateEscapeRouteReduction(
        Board board,
        Snake opponent,
        IEnumerable<Snake> allSnakes)
    {
        // Count how many safe moves opponent has
        var safeMoves = MoveValidator.GetSafeMoves(board, opponent, allSnakes);

        // Also calculate how many "spacious" moves they have (moves with good space)
        int spaciousMoveCount = 0;
        int minRequiredSpace = opponent.Length;

        foreach (var move in safeMoves)
        {
            var nextPos = MoveValidator.GetNextPosition(opponent.Head, move);
            var space = CalculateReachableSpace(nextPos, board, opponent, allSnakes);

            if (space >= minRequiredSpace)
            {
                spaciousMoveCount++;
            }
        }

        // Reduction score:
        // - 4 safe moves = 0 reduction (no pressure)
        // - 3 safe moves = 1 reduction
        // - 2 safe moves = 2 reduction
        // - 1 safe move = 3 reduction (heavily trapped)
        // - Bonus if spacious moves are limited
        int safeMovesReduction = 4 - safeMoves.Count;
        int spaciousMovesReduction = 4 - spaciousMoveCount;

        return Math.Max(0, safeMovesReduction + spaciousMovesReduction);
    }

    /// <summary>
    /// Calculates reachable space using flood fill (optimized version).
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
    /// Checks if a position is accessible (lenient check for future predictions).
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

        // Check all snake bodies (skip tails as they move)
        foreach (var s in allSnakes)
        {
            var body = s.Body.ToList();
            for (int i = 0; i < body.Count - 1; i++)
            {
                if (body[i].X == position.X && body[i].Y == position.Y)
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
