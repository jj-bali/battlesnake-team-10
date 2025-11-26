namespace Starter.Api.Algorithms.AStar;

/// <summary>
/// A* pathfinding algorithm implementation for Battlesnake.
/// Uses Manhattan distance heuristic for 4-directional grid movement.
/// </summary>
public class AStarPathfinder
{
    /// <summary>
    /// Finds the shortest path from start to goal using A* algorithm.
    /// </summary>
    /// <param name="start">Starting coordinate</param>
    /// <param name="goal">Goal coordinate</param>
    /// <param name="walkableGrid">2D grid where true = walkable, false = obstacle</param>
    /// <param name="boardWidth">Width of the board</param>
    /// <param name="boardHeight">Height of the board</param>
    /// <returns>List of coordinates from start to goal, or null if no path exists</returns>
    public List<Coordinate>? FindPath(
        Coordinate start,
        Coordinate goal,
        bool[,] walkableGrid,
        int boardWidth,
        int boardHeight)
    {
        // Validate coordinates
        if (!IsValid(start, boardWidth, boardHeight) || !IsValid(goal, boardWidth, boardHeight))
            return null;

        if (!walkableGrid[start.Y, start.X] || !walkableGrid[goal.Y, goal.X])
            return null;

        // Initialize data structures
        var openSet = new PriorityQueue<PathNode, int>();
        var closedSet = new HashSet<Coordinate>();
        var allNodes = new Dictionary<Coordinate, PathNode>();

        // Create start node
        var startNode = new PathNode(start)
        {
            GCost = 0,
            HCost = ManhattanDistance(start, goal)
        };
        allNodes[start] = startNode;
        openSet.Enqueue(startNode, startNode.FCost);

        while (openSet.Count > 0)
        {
            // Get node with lowest F-cost
            var currentNode = openSet.Dequeue();

            // Check if we reached the goal
            if (currentNode.Position == goal)
            {
                return ReconstructPath(currentNode);
            }

            // Add to closed set
            closedSet.Add(currentNode.Position);

            // Check all neighbors
            foreach (var neighborPos in GetNeighbors(currentNode.Position, boardWidth, boardHeight))
            {
                // Skip if not walkable or already visited
                if (!walkableGrid[neighborPos.Y, neighborPos.X] || closedSet.Contains(neighborPos))
                    continue;

                // Calculate tentative G-cost
                int tentativeGCost = currentNode.GCost + 1; // Movement cost is 1

                // Get or create neighbor node
                if (!allNodes.TryGetValue(neighborPos, out var neighborNode))
                {
                    neighborNode = new PathNode(neighborPos)
                    {
                        HCost = ManhattanDistance(neighborPos, goal)
                    };
                    allNodes[neighborPos] = neighborNode;
                }

                // Update if this path is better
                if (tentativeGCost < neighborNode.GCost || neighborNode.GCost == 0)
                {
                    neighborNode.GCost = tentativeGCost;
                    neighborNode.Parent = currentNode;

                    // Add to open set if not already there
                    if (!closedSet.Contains(neighborPos))
                    {
                        openSet.Enqueue(neighborNode, neighborNode.FCost);
                    }
                }
            }
        }

        // No path found
        return null;
    }

    /// <summary>
    /// Calculates Manhattan distance between two coordinates.
    /// </summary>
    private int ManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Gets all valid neighbors (up, down, left, right).
    /// </summary>
    private IEnumerable<Coordinate> GetNeighbors(Coordinate coord, int width, int height)
    {
        var neighbors = new List<Coordinate>
        {
            new(coord.X, coord.Y + 1), // up
            new(coord.X, coord.Y - 1), // down
            new(coord.X - 1, coord.Y), // left
            new(coord.X + 1, coord.Y)  // right
        };

        return neighbors.Where(n => IsValid(n, width, height));
    }

    /// <summary>
    /// Checks if coordinate is within board bounds.
    /// </summary>
    private bool IsValid(Coordinate coord, int width, int height)
    {
        return coord.X >= 0 && coord.X < width && coord.Y >= 0 && coord.Y < height;
    }

    /// <summary>
    /// Reconstructs the path from goal to start using parent pointers.
    /// </summary>
    private List<Coordinate> ReconstructPath(PathNode goalNode)
    {
        var path = new List<Coordinate>();
        var current = goalNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse(); // Reverse to get start -> goal order
        return path;
    }
}
