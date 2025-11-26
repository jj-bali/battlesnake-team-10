namespace Starter.Api.GameLogic;

public class SizeOptimizer
{
    /// <summary>
    /// Calculate the optimal snake size based on board size and opponents.
    /// Strategy:
    /// - Larger board = can afford bigger snake
    /// - More opponents = need to stay competitive
    /// - Smaller board = stay lean for mobility
    /// </summary>
    public static int CalculateOptimalSize(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        var opponents = allSnakes.Where(s => s.Id != you.Id).ToList();

        // Base calculation: Grid space availability
        int totalGridCells = board.Width * board.Height;

        // Factor 1: Board size determines base optimal size
        // Use square root as a heuristic - larger boards allow proportionally bigger snakes
        // For 11x11 (121 cells): sqrt(121) = 11
        // For 7x7 (49 cells): sqrt(49) = 7
        int boardSizeOptimal = (int)Math.Sqrt(totalGridCells);

        // Factor 2: Adjust based on number of opponents
        // More opponents = less free space, stay smaller
        // Fewer opponents = more space, can grow bigger
        int opponentCount = opponents.Count;
        int opponentPenalty = opponentCount * 2; // Reduce optimal size by 2 per opponent

        // Factor 3: Consider largest opponent size
        // We want to be slightly bigger than the largest threat
        int largestOpponentSize = opponents.Any()
            ? opponents.Max(s => s.Length)
            : 3; // Default starting size

        // Factor 4: Consider average opponent size for competitive positioning
        double averageOpponentSize = opponents.Any()
            ? opponents.Average(s => s.Length)
            : 3;

        // Calculate optimal size using weighted formula:
        // - 40% based on board size
        // - 30% based on largest opponent + buffer
        // - 30% based on average opponent size
        double optimalSize =
            (boardSizeOptimal * 0.4) +
            ((largestOpponentSize + 2) * 0.3) + // +2 buffer to dominate
            (averageOpponentSize * 0.3);

        // Apply opponent penalty
        optimalSize -= opponentPenalty;

        // Constraints:
        // - Minimum size: Stay at least as big as largest opponent, or 5 (whichever is larger)
        int minSize = Math.Max(largestOpponentSize, 5);

        // - Maximum size: Don't exceed 60% of board size to maintain mobility
        int maxSize = (int)(totalGridCells * 0.6);

        // Apply constraints
        int finalOptimalSize = (int)Math.Round(optimalSize);
        finalOptimalSize = Math.Max(finalOptimalSize, minSize);
        finalOptimalSize = Math.Min(finalOptimalSize, maxSize);

        return finalOptimalSize;
    }

    /// <summary>
    /// Determine if we should seek food based on size optimization.
    /// Returns true if we're below optimal size, false if we've reached it.
    /// </summary>
    public static bool ShouldGrowForOptimalSize(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        int optimalSize = CalculateOptimalSize(board, you, allSnakes);

        // Allow small buffer above optimal (1-2 extra segments is ok)
        int maxAcceptableSize = optimalSize + 2;

        return you.Length < maxAcceptableSize;
    }

    /// <summary>
    /// Determine strategy: growth mode vs survival mode
    /// </summary>
    public static SizeStrategy GetSizeStrategy(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        int optimalSize = CalculateOptimalSize(board, you, allSnakes);
        int currentSize = you.Length;

        var opponents = allSnakes.Where(s => s.Id != you.Id).ToList();
        int largestOpponent = opponents.Any() ? opponents.Max(s => s.Length) : 0;

        // Critical: If we're smaller than largest opponent, MUST grow
        if (currentSize < largestOpponent)
        {
            return new SizeStrategy
            {
                Mode = GrowthMode.Aggressive,
                OptimalSize = optimalSize,
                Reason = $"Smaller than largest opponent ({largestOpponent})"
            };
        }

        // If we're below optimal, grow
        if (currentSize < optimalSize)
        {
            return new SizeStrategy
            {
                Mode = GrowthMode.Moderate,
                OptimalSize = optimalSize,
                Reason = "Below optimal size"
            };
        }

        // If we're within buffer of optimal, maintain
        if (currentSize <= optimalSize + 2)
        {
            return new SizeStrategy
            {
                Mode = GrowthMode.Maintain,
                OptimalSize = optimalSize,
                Reason = "At optimal size"
            };
        }

        // If we're too big, avoid growth
        return new SizeStrategy
        {
            Mode = GrowthMode.Avoid,
            OptimalSize = optimalSize,
            Reason = "Above optimal size - mobility risk"
        };
    }
}

public class SizeStrategy
{
    public GrowthMode Mode { get; set; }
    public int OptimalSize { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public enum GrowthMode
{
    Aggressive,  // Must grow - seek food actively
    Moderate,    // Should grow - seek food when safe
    Maintain,    // At optimal size - only eat when health low
    Avoid        // Too big - avoid food unless starving
}
