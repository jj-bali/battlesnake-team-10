namespace Starter.Api.Pathfinding;

public class AStarPathfinder
{
    private class PathNode
    {
        public Coordinate Position { get; set; }
        public int GCost { get; set; } // Distance from start
        public int HCost { get; set; } // Heuristic distance to goal
        public int FCost => GCost + HCost;
        public PathNode? Parent { get; set; }

        public PathNode(Coordinate position)
        {
            Position = position;
        }
    }

    public static List<Coordinate>? FindPath(
        Coordinate start,
        Coordinate goal,
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes)
    {
        var openList = new List<PathNode>();
        var closedList = new HashSet<string>();

        var startNode = new PathNode(start)
        {
            GCost = 0,
            HCost = ManhattanDistance(start, goal)
        };

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Get node with lowest FCost
            var currentNode = openList.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();

            // Check if we reached the goal
            if (currentNode.Position.X == goal.X && currentNode.Position.Y == goal.Y)
            {
                return ReconstructPath(currentNode);
            }

            openList.Remove(currentNode);
            closedList.Add(GetKey(currentNode.Position));

            // Check all neighbors
            foreach (var neighbor in GetNeighbors(currentNode.Position, board, you, allSnakes))
            {
                var neighborKey = GetKey(neighbor);
                if (closedList.Contains(neighborKey))
                {
                    continue;
                }

                var newGCost = currentNode.GCost + 1;
                var existingNode = openList.FirstOrDefault(n =>
                    n.Position.X == neighbor.X && n.Position.Y == neighbor.Y);

                if (existingNode == null)
                {
                    openList.Add(new PathNode(neighbor)
                    {
                        GCost = newGCost,
                        HCost = ManhattanDistance(neighbor, goal),
                        Parent = currentNode
                    });
                }
                else if (newGCost < existingNode.GCost)
                {
                    existingNode.GCost = newGCost;
                    existingNode.Parent = currentNode;
                }
            }
        }

        return null; // No path found
    }

    private static List<Coordinate> GetNeighbors(
        Coordinate position,
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes)
    {
        var neighbors = new List<Coordinate>
        {
            new(position.X, position.Y + 1), // Up
            new(position.X, position.Y - 1), // Down
            new(position.X - 1, position.Y), // Left
            new(position.X + 1, position.Y)  // Right
        };

        // Filter out invalid positions
        return neighbors.Where(n => IsValidPosition(n, board, you, allSnakes)).ToList();
    }

    private static bool IsValidPosition(
        Coordinate position,
        Board board,
        Snake you,
        IEnumerable<Snake> allSnakes)
    {
        // Check boundaries
        if (position.X < 0 || position.X >= board.Width ||
            position.Y < 0 || position.Y >= board.Height)
        {
            return false;
        }

        // Check own body (except tail, which will move away each turn)
        // Note: This is for pathfinding over multiple moves, not immediate validation
        var yourBody = you.Body.ToList();
        for (int i = 0; i < yourBody.Count - 1; i++)
        {
            if (yourBody[i].X == position.X && yourBody[i].Y == position.Y)
            {
                return false;
            }
        }

        // Check other snakes' bodies
        foreach (var snake in allSnakes.Where(s => s.Id != you.Id))
        {
            foreach (var segment in snake.Body)
            {
                if (segment.X == position.X && segment.Y == position.Y)
                {
                    return false;
                }
            }

            // Avoid positions adjacent to larger or equal snakes' heads
            // Equal size = both die, so avoid. Only be aggressive against smaller snakes.
            if (snake.Length >= you.Length)
            {
                var head = snake.Head;
                if (Math.Abs(head.X - position.X) + Math.Abs(head.Y - position.Y) == 1)
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

    private static int ManhattanDistance(Coordinate a, Coordinate b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static List<Coordinate> ReconstructPath(PathNode endNode)
    {
        var path = new List<Coordinate>();
        var current = endNode;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private static string GetKey(Coordinate coord)
    {
        return $"{coord.X},{coord.Y}";
    }
}
