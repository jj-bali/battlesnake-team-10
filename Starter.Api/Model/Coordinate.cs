namespace Starter.Api;

/// <summary>
/// Coordinate on the 2D grid game board.
/// Coordinates begin at zero.
/// </summary>
public class Coordinate : IEquatable<Coordinate>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Required for HashSet and Dictionary usage in A* pathfinding
    public override bool Equals(object? obj) => Equals(obj as Coordinate);

    public bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Coordinate? left, Coordinate? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Coordinate? left, Coordinate? right)
        => !(left == right);
}