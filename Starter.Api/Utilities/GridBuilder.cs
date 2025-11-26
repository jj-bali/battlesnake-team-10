namespace Starter.Api;

/// <summary>
/// Converts Battlesnake Board state to a walkability grid for pathfinding.
/// </summary>
public class GridBuilder
{
    /// <summary>
    /// Builds a walkability grid from the current board state.
    /// </summary>
    /// <param name="board">The game board</param>
    /// <param name="you">Your snake</param>
    /// <returns>2D array where true = walkable, false = obstacle</returns>
    public bool[,] BuildWalkableGrid(Board board, Snake you)
    {
        // Initialize all cells as walkable
        var grid = new bool[board.Height, board.Width];
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                grid[y, x] = true;
            }
        }

        // Mark all snake bodies as unwalkable
        foreach (var snake in board.Snakes)
        {
            // Mark body segments as unwalkable
            foreach (var segment in snake.Body)
            {
                if (segment.X >= 0 && segment.X < board.Width &&
                    segment.Y >= 0 && segment.Y < board.Height)
                {
                    grid[segment.Y, segment.X] = false;
                }
            }

            // Special handling: tails are walkable UNLESS the snake is growing
            // A snake is growing when health == 100 (just ate food)
            var tail = snake.Body.LastOrDefault();
            if (tail != null && snake.Health < 100)
            {
                if (tail.X >= 0 && tail.X < board.Width &&
                    tail.Y >= 0 && tail.Y < board.Height)
                {
                    grid[tail.Y, tail.X] = true;
                }
            }
        }

        // Mark hazards as unwalkable
        foreach (var hazard in board.Hazards)
        {
            if (hazard.X >= 0 && hazard.X < board.Width &&
                hazard.Y >= 0 && hazard.Y < board.Height)
            {
                grid[hazard.Y, hazard.X] = false;
            }
        }

        return grid;
    }
}
