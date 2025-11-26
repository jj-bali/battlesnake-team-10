namespace Starter.Api;

/// <summary>
/// Helper methods for converting between coordinates and direction strings.
/// </summary>
public static class DirectionHelper
{
    /// <summary>
    /// Converts a coordinate movement to a direction string.
    /// </summary>
    /// <param name="from">Starting coordinate</param>
    /// <param name="to">Destination coordinate</param>
    /// <returns>"up", "down", "left", or "right"</returns>
    public static string GetDirection(Coordinate from, Coordinate to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (dx == 1) return "right";
        if (dx == -1) return "left";
        if (dy == 1) return "up";
        if (dy == -1) return "down";

        // Fallback - should not happen with valid adjacent coordinates
        return "up";
    }

    /// <summary>
    /// Gets the coordinate resulting from moving in a direction.
    /// </summary>
    /// <param name="from">Starting coordinate</param>
    /// <param name="direction">"up", "down", "left", or "right"</param>
    /// <returns>New coordinate after the move</returns>
    public static Coordinate GetCoordinateInDirection(Coordinate from, string direction)
    {
        return direction.ToLower() switch
        {
            "up" => new Coordinate(from.X, from.Y + 1),
            "down" => new Coordinate(from.X, from.Y - 1),
            "left" => new Coordinate(from.X - 1, from.Y),
            "right" => new Coordinate(from.X + 1, from.Y),
            _ => from // Invalid direction, return original
        };
    }
}
