# Claude Battlesnake (.NET 9 C# Webserver)

This repository <REPO_URL> contains Battlesnake web server written in **C# (.NET 9)**.
It is designed to compete autonomously in the [Battlesnake](https://play.battlesnake.com/) tournaments by exposing a compliant web API.
You are currentlyin the cloned projec of the repository listed above.

---

## 📖 Overview

- **Framework:** ASP.NET 9 (C#)
- **Purpose:** Compete in the Battlesnake web-based multiplayer game
- **Cloud Deployment:** Google Cloud Platform (GCP)

### Prerequisites

- [.NET 9 SDK (preview or current)](https://dotnet.microsoft.com/en-us/download)
- (Optional for local testing) [ngrok](https://ngrok.com/)
- GCP project for deployment

## Game Rules and Spec
- Game rules: https://docs.battlesnake.com/rules
- API reference: https://docs.battlesnake.com/api


## Code enhancents
Our BattleSnake is designed to follow game rules described above but also incorporate :

- Use A* Pathfinding to select the shortest safe path to food or to evade threats. https://theory.stanford.edu/~amitp/GameProgramming/AStarComparison.html
- Never eat yourself (no moving onto your own body).
- Never move out of bounds(grid) (check all moves for legal coordinates).
- Food and Opponent Targeting:
- If health < 5, rioritize moving towards the nearest food, otherwise move towards the closest smaller snake (for aggression or tactical positioning), 
- Always prefer the safest route (using A* for shortest path) and avoid traps or collisions.
- Only consider moves that are safe from immediate death and do not box yourself in.
