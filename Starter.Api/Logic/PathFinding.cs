namespace Starter.Api.Logic;

/// <summary>
/// A* pathfinding implementation for finding shortest safe paths
/// </summary>
public class PathFinding
{
    private class Node
    {
        public Coordinate Position { get; set; }
        public Node? Parent { get; set; }
        public int G { get; set; } // Cost from start
        public int H { get; set; } // Heuristic cost to goal
        public int F => G + H; // Total cost

        public Node(Coordinate position, Node? parent, int g, int h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    /// <summary>
    /// Finds the shortest path from start to goal using A* algorithm
    /// </summary>
    /// <param name="start">Starting coordinate</param>
    /// <param name="goal">Goal coordinate</param>
    /// <param name="board">Game board</param>
    /// <param name="you">Your snake</param>
    /// <returns>List of coordinates representing the path, or empty list if no path found</returns>
    public static List<Coordinate> FindPath(Coordinate start, Coordinate goal, Board board, Snake you)
    {
        var openList = new List<Node>();
        var closedList = new HashSet<string>();

        var startNode = new Node(start, null, 0, CalculateManhattanDistance(start, goal));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Get node with lowest F score
            var currentNode = openList.OrderBy(n => n.F).ThenBy(n => n.H).First();
            openList.Remove(currentNode);

            // Check if we reached the goal
            if (currentNode.Position.X == goal.X && currentNode.Position.Y == goal.Y)
            {
                return ReconstructPath(currentNode);
            }

            closedList.Add($"{currentNode.Position.X},{currentNode.Position.Y}");

            // Check all neighbors
            var neighbors = GetNeighbors(currentNode.Position, board);
            foreach (var neighbor in neighbors)
            {
                var neighborKey = $"{neighbor.X},{neighbor.Y}";
                if (closedList.Contains(neighborKey))
                    continue;

                // Skip if position is not safe (except for the goal position which might be food)
                if (!IsSafePosition(neighbor, board, you) && !(neighbor.X == goal.X && neighbor.Y == goal.Y))
                    continue;

                var g = currentNode.G + 1;
                var h = CalculateManhattanDistance(neighbor, goal);

                var existingNode = openList.FirstOrDefault(n => n.Position.X == neighbor.X && n.Position.Y == neighbor.Y);
                if (existingNode != null)
                {
                    if (g < existingNode.G)
                    {
                        existingNode.G = g;
                        existingNode.Parent = currentNode;
                    }
                }
                else
                {
                    openList.Add(new Node(neighbor, currentNode, g, h));
                }
            }
        }

        // No path found
        return new List<Coordinate>();
    }

    /// <summary>
    /// Calculates Manhattan distance between two coordinates
    /// </summary>
    public static int CalculateManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    /// <summary>
    /// Gets valid neighboring coordinates
    /// </summary>
    private static List<Coordinate> GetNeighbors(Coordinate position, Board board)
    {
        var neighbors = new List<Coordinate>();
        var directions = new[]
        {
            new { X = 0, Y = 1 },  // up
            new { X = 0, Y = -1 }, // down
            new { X = -1, Y = 0 }, // left
            new { X = 1, Y = 0 }   // right
        };

        foreach (var dir in directions)
        {
            var newX = position.X + dir.X;
            var newY = position.Y + dir.Y;

            // Check bounds
            if (newX >= 0 && newX < board.Width && newY >= 0 && newY < board.Height)
            {
                neighbors.Add(new Coordinate(newX, newY));
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Checks if a position is safe (no snakes)
    /// </summary>
    private static bool IsSafePosition(Coordinate position, Board board, Snake you)
    {
        // Check all snakes on the board
        foreach (var snake in board.Snakes)
        {
            // Check snake body (excluding tail as it will move)
            var bodySegments = snake.Body.ToList();
            for (int i = 0; i < bodySegments.Count - 1; i++)
            {
                if (bodySegments[i].X == position.X && bodySegments[i].Y == position.Y)
                    return false;
            }

            // Check for potential head-to-head collisions with larger or equal snakes
            if (snake.Id != you.Id)
            {
                var snakeHead = snake.Head;
                var distance = CalculateManhattanDistance(position, snakeHead);

                // If we're one move away from another snake's head and they're >= our size, avoid
                if (distance == 1 && snake.Length >= you.Length)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Reconstructs the path from the goal node back to start
    /// </summary>
    private static List<Coordinate> ReconstructPath(Node goalNode)
    {
        var path = new List<Coordinate>();
        var current = goalNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}