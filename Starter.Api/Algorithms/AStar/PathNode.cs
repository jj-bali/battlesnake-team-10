namespace Starter.Api.Algorithms.AStar;

/// <summary>
/// Represents a node in the A* pathfinding algorithm.
/// </summary>
public class PathNode
{
    /// <summary>
    /// The coordinate position of this node.
    /// </summary>
    public Coordinate Position { get; set; }

    /// <summary>
    /// G-Cost: Distance from start node (actual cost).
    /// </summary>
    public int GCost { get; set; }

    /// <summary>
    /// H-Cost: Heuristic distance to goal (estimated cost).
    /// </summary>
    public int HCost { get; set; }

    /// <summary>
    /// F-Cost: Total cost (GCost + HCost).
    /// Used for priority in the open set.
    /// </summary>
    public int FCost => GCost + HCost;

    /// <summary>
    /// Parent node in the path (for reconstruction).
    /// </summary>
    public PathNode? Parent { get; set; }

    public PathNode(Coordinate position)
    {
        Position = position;
        GCost = 0;
        HCost = 0;
        Parent = null;
    }
}
