using Starter.Api.Logic;
using Starter.Api.Requests;
using Starter.Api.Responses;

var builder = WebApplication.CreateBuilder(args);
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
        Author = "Team 10 - A* Pathfinder",
        Color = "#00FF00",
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
app.MapPost("/move", (GameStatusRequest gameStatusRequest) =>
{
    // Extract game state
    var board = gameStatusRequest.Board;
    var you = gameStatusRequest.You;

    // Use strategy to choose the best move
    var move = Strategy.ChooseBestMove(you, board);
    var shout = Strategy.GenerateShout(you, board, move);

    return new MoveResponse
    {
        Move = move,
        Shout = shout
    };
});

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