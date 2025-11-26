namespace Starter.Api;

/// <summary>
/// Extension methods for Coordinate operations.
/// </summary>
public static class CoordinateExtensions
{
    /// <summary>
    /// Gets all 4 adjacent neighbors (up, down, left, right).
    /// No diagonal movement.
    /// </summary>
    public static IEnumerable<Coordinate> GetNeighbors(this Coordinate coord, Board board)
    {
        var neighbors = new List<Coordinate>
        {
            new(coord.X, coord.Y + 1), // up
            new(coord.X, coord.Y - 1), // down
            new(coord.X - 1, coord.Y), // left
            new(coord.X + 1, coord.Y)  // right
        };

        return neighbors.Where(n => n.IsInBounds(board));
    }

    /// <summary>
    /// Checks if coordinate is within board boundaries.
    /// </summary>
    public static bool IsInBounds(this Coordinate coord, Board board)
    {
        return coord.X >= 0 && coord.X < board.Width &&
               coord.Y >= 0 && coord.Y < board.Height;
    }

    /// <summary>
    /// Calculates Manhattan distance between two coordinates.
    /// Formula: |x1 - x2| + |y1 - y2|
    /// </summary>
    public static int ManhattanDistanceTo(this Coordinate from, Coordinate to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }
}
