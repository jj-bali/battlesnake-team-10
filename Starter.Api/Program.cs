using Starter.Api;
using Starter.Api.Algorithms.AStar;
using Starter.Api.Requests;
using Starter.Api.Responses;

var builder = WebApplication.CreateBuilder(args);

// Register A* pathfinding services
builder.Services.AddSingleton<GridBuilder>();
builder.Services.AddSingleton<AStarPathfinder>();

var app = builder.Build();
app.UseHttpsRedirection();

/// <summary>
/// This request will be made periodically to retrieve information about your Battlesnake,
/// including its display options, author, etc.
/// </summary>
app.MapGet("/", () =>
{
    return new InitResponse
    {
        ApiVersion = "1",
        Author = "",
        Color = "#FFFFFF",
        Head = "default",
        Tail = "default"
    };
});

/// <summary>
/// Your Battlesnake will receive this request when it has been entered into a new game.
/// Every game has a unique ID that can be used to allocate resources or data you may need.
/// Your response to this request will be ignored.
/// </summary>
app.MapPost("/start", (GameStatusRequest gameStatusRequest) =>
{
    Results.Ok();
});

/// <summary>
/// This request will be sent for every turn of the game.
/// Use the information provided to determine how your
/// Battlesnake will move on that turn, either up, down, left, or right.
/// </summary>
app.MapPost("/move", (GameStatusRequest request, GridBuilder gridBuilder, AStarPathfinder pathfinder) =>
{
    // Build walkability grid from current board state
    var grid = gridBuilder.BuildWalkableGrid(request.Board, request.You);

    // Default to a random safe move
    string move = "up";

    // Find path to nearest food
    var nearestFood = request.Board.Food
        .OrderBy(f => request.You.Head.ManhattanDistanceTo(f))
        .FirstOrDefault();
    Console.WriteLine("The current food board is: ", request.Board.Food);
    Console.WriteLine("The nearest food is: ", nearestFood);
    if (nearestFood != null)
    {
        var path = pathfinder.FindPath(
            request.You.Head,
            nearestFood,
            grid,
            request.Board.Width,
            request.Board.Height);

        if (path != null && path.Count > 1)
        {
            // path[0] is current position, path[1] is next step
            move = DirectionHelper.GetDirection(path[0], path[1]);
        }
        else
        {
            // No path to food, try to find any safe move
            var safeMove = FindAnySafeMove(request.You.Head, grid, request.Board);
            if (safeMove != null)
                move = safeMove;
        }
    }
    else
    {
        // No food available, find any safe move
        var safeMove = FindAnySafeMove(request.You.Head, grid, request.Board);
        if (safeMove != null)
            move = safeMove;
    }

    return new MoveResponse
    {
        Move = move,
        Shout = "A* pathfinding!"
    };
});

// Helper function to find any safe move
static string? FindAnySafeMove(Coordinate head, bool[,] grid, Board board)
{
    var directions = new[] { "up", "down", "left", "right" };

    foreach (var dir in directions)
    {
        var nextPos = DirectionHelper.GetCoordinateInDirection(head, dir);

        if (nextPos.X >= 0 && nextPos.X < board.Width &&
            nextPos.Y >= 0 && nextPos.Y < board.Height &&
            grid[nextPos.Y, nextPos.X])
        {
            return dir;
        }
    }

    return null; // No safe moves available
}

/// <summary>
/// Your Battlesnake will receive this request whenever a game it was playing has ended.
/// Use it to learn how your Battlesnake won or lost and deallocated any server-side resources.
/// Your response to this request will be ignored.
/// </summary>
app.MapPost("/end", (GameStatusRequest gameStatusRequest) =>
{
    Results.Ok();
});

app.Run();