namespace Starter.Api.GameLogic;

public class MoveValidator
{
    public static List<string> GetSafeMoves(Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        var allMoves = new List<string> { "up", "down", "left", "right" };
        var safeMoves = new List<string>();

        foreach (var move in allMoves)
        {
            var nextPosition = GetNextPosition(you.Head, move);

            if (IsSafeMove(nextPosition, board, you, allSnakes))
            {
                safeMoves.Add(move);
            }
        }

        return safeMoves;
    }

    public static Coordinate GetNextPosition(Coordinate current, string direction)
    {
        return direction switch
        {
            "up" => new Coordinate(current.X, current.Y + 1),
            "down" => new Coordinate(current.X, current.Y - 1),
            "left" => new Coordinate(current.X - 1, current.Y),
            "right" => new Coordinate(current.X + 1, current.Y),
            _ => current
        };
    }

    public static bool IsSafeMove(Coordinate position, Board board, Snake you, IEnumerable<Snake> allSnakes)
    {
        // Check boundaries
        if (!IsWithinBounds(position, board))
        {
            return false;
        }

        // Check if we're moving to a food position (tail won't move if we eat)
        bool willEatFood = board.Food != null && board.Food.Any(f => f.X == position.X && f.Y == position.Y);

        // Check self-collision (don't hit own body)
        if (CollidesWithSelf(position, you, willEatFood))
        {
            return false;
        }

        // Check collision with other snakes
        if (CollidesWithOtherSnakes(position, you, allSnakes))
        {
            return false;
        }

        // Check hazards
        if (IsHazard(position, board))
        {
            return false;
        }

        return true;
    }

    private static bool IsWithinBounds(Coordinate position, Board board)
    {
        return position.X >= 0 && position.X < board.Width &&
               position.Y >= 0 && position.Y < board.Height;
    }

    private static bool CollidesWithSelf(Coordinate position, Snake you, bool willEatFood)
    {
        var bodyList = you.Body.ToList();

        // If we're eating food, the tail won't move, so check all segments
        // Otherwise, tail will move away, so we can skip checking it
        int segmentsToCheck = willEatFood ? bodyList.Count : bodyList.Count - 1;

        for (int i = 0; i < segmentsToCheck; i++)
        {
            if (bodyList[i].X == position.X && bodyList[i].Y == position.Y)
            {
                return true;
            }
        }
        return false;
    }

    private static bool CollidesWithOtherSnakes(Coordinate position, Snake you, IEnumerable<Snake> allSnakes)
    {
        foreach (var snake in allSnakes.Where(s => s.Id != you.Id))
        {
            // Check body collision
            foreach (var segment in snake.Body)
            {
                if (segment.X == position.X && segment.Y == position.Y)
                {
                    return true;
                }
            }

            // Avoid head-to-head collision with larger or equal snakes
            var head = snake.Head;
            var distance = Math.Abs(head.X - position.X) + Math.Abs(head.Y - position.Y);

            if (distance == 1 && snake.Length >= you.Length)
            {
                return true; // Too risky to move adjacent to a larger/equal snake's head
            }
        }
        return false;
    }

    private static bool IsHazard(Coordinate position, Board board)
    {
        if (board.Hazards == null)
        {
            return false;
        }

        return board.Hazards.Any(h => h.X == position.X && h.Y == position.Y);
    }

    public static string GetDirectionFromCoordinates(Coordinate from, Coordinate to)
    {
        if (to.Y > from.Y) return "up";
        if (to.Y < from.Y) return "down";
        if (to.X < from.X) return "left";
        if (to.X > from.X) return "right";
        return "up"; // Default fallback
    }
}
