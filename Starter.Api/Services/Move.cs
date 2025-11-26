namespace Starter.Api.Services;

public class Move
{

    private Snake snake;
    
    public Move(Snake mySnake)
    {
        this.snake = mySnake;
    }

    // Provides invalid directions
    public List<string> InvalidDirections()
    {
        return EatMyself();
    }
    
    // Provides directions in which the snake can eat itself
    public List<string> EatMyself()
    {
        // (x, y) coordinates
        // (0, 1) Up
        // (0, -1) Down
        // (1, 0) Right
        //(-1, 0) Left

        const int width = 11;
        const int height = 11;
        
        var invalidDirections = new List<string>();
        var currPos = snake.Head;
        var hashedCoordinates = snake.Body.ToHashSet();

        // Check Up
        var upCoordinate = new Coordinate(currPos.X, currPos.Y + 1);
        if (upCoordinate.Y >= height || hashedCoordinates.Contains(upCoordinate))
        {
            invalidDirections.Add("up");
        }

        // Check Down
        var downCoordinate = new Coordinate(currPos.X, currPos.Y - 1);
        if (downCoordinate.Y < 0 || hashedCoordinates.Contains(downCoordinate))
        {
            invalidDirections.Add("down");
        }

        // Check Right
        var rightCoordinate = new Coordinate(currPos.X + 1, currPos.Y);
        if (rightCoordinate.X >= width || hashedCoordinates.Contains(rightCoordinate))
        {
            invalidDirections.Add("right");
        }

        // Check Left
        var leftCoordinate = new Coordinate(currPos.X - 1, currPos.Y);
        if (leftCoordinate.X < 0 || hashedCoordinates.Contains(leftCoordinate))
        {
            invalidDirections.Add("left");
        }

        return invalidDirections;
    }

}