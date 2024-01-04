using System;
using System.Collections.Generic;
using System.Linq;
using MiningGameMapGenerationTest.MapGeneration.Tiles;

namespace MiningGameMapGenerationTest.MapGeneration;

public class MapGenerator
{
    public int Seed { get; init; } = new Random().Next();
    public int Width { get; init; }
    public int Height { get; init; }

    private static readonly Position DefaultPosition = new(0, 0);
    public Position MainRoom { get; init; } = DefaultPosition;
    public Position RedBase { get; init; } = DefaultPosition;
    public Position YellowBase { get; init; } = DefaultPosition;
    public Position GreenBase { get; init; } = DefaultPosition;
    public Position BlueBase { get; init; } = DefaultPosition;

    public Tile[][] Grid { get; set; } = Array.Empty<Tile[]>();

    public void GenerateAll()
    {
        GenerateBasics();
        GeneratePaths();
        GenerateOres();
    }

    public void GenerateBasics()
    {
        Grid = new Tile[50][];
        for (var y = 0; y < Height; y++)
        {
            var row = new Tile[50];
            Grid[y] = row;
            for (var x = 0; x < Width; x++)
            {
                row[x] = new DirtTile();
            }
        }
        
        GenerateBase(MainRoom, BaseType.General);
        GenerateBase(RedBase, BaseType.Red);
        GenerateBase(YellowBase, BaseType.Yellow);
        GenerateBase(GreenBase, BaseType.Green);
        GenerateBase(BlueBase, BaseType.Blue);
    }

    private void GenerateBase(Position position, BaseType type)
    {
        GenerateBase(3, 3, position, type);
    }

    private void GenerateBase(int height, int width, Position position, BaseType type)
    {
        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                Grid[position.Y + i][position.X + j] = new BaseTile
                {
                    BaseType = type
                };
            }
        }
    }

    public void GeneratePaths()
    {
        var random = new Random(Seed);

        for (var i = 0; i < 2; i++)
        {
            var includeTracks = i == 0;
            GeneratePath(random, RedBase, MainRoom, includeTracks);
            GeneratePath(random, YellowBase, MainRoom, includeTracks);
            GeneratePath(random, GreenBase, MainRoom, includeTracks);
            GeneratePath(random, BlueBase, MainRoom, includeTracks);
        }

        var bases = new []{RedBase, YellowBase, GreenBase, BlueBase};
        foreach (var origin in bases)
        {
            foreach (var target in bases)
            {
                if (origin == target) continue;
                GeneratePath(random, origin, target, random.Next(3) == 0);
            }
        }
        
        FixAllTracks();
    }

    private void GeneratePath(Random random, Position origin, Position target, bool includeTracks)
    {
        // Find starting direction
        IList<Direction> availableDirections = new List<Direction>();
        if (origin.Y <= target.Y)
        {
            availableDirections.Add(Direction.South);
        }
        if (origin.Y >= target.Y)
        {
            availableDirections.Add(Direction.North);
        }
        if (origin.X <= target.X)
        {
            availableDirections.Add(Direction.East);
        }
        if (origin.X >= target.X)
        {
            availableDirections.Add(Direction.West);
        }
        
        var currentDirection = availableDirections[random.Next(availableDirections.Count)];
        var currentPosition = new Position(origin.X + 1, origin.Y + 1);
        
        // Create paths
        for (var i = 0; i < random.Next(Height, Height * 2); i++)
        {
            var diffX = target.X - currentPosition.X;
            var diffY = target.Y - currentPosition.Y;

            var preferredDirection = GetPreferredDirection(diffX, diffY);
            var selectedPercentage = random.Next(100);

            var possibleDirections = new List<Direction> { Direction.North, Direction.East, Direction.South, Direction.West };
            var canGoNorth = currentPosition.Y > 0;
            var canGoEast = currentPosition.X < Height - 1;
            var canGoSouth = currentPosition.Y < Width - 1;
            var canGoWest = currentPosition.X > 0;
            if (!canGoNorth) possibleDirections.Remove(Direction.North);
            if (!canGoEast) possibleDirections.Remove(Direction.East);
            if (!canGoSouth) possibleDirections.Remove(Direction.South);
            if (!canGoWest) possibleDirections.Remove(Direction.West);

            var oppositeDirection = currentDirection switch
            {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                _ => throw new ArgumentOutOfRangeException()
            };
            possibleDirections.Remove(oppositeDirection);

            if (possibleDirections.Count == 1)
            {
                currentDirection = possibleDirections[0];
            }
            else if (possibleDirections.Count == 2)
            {
                currentDirection = selectedPercentage < 50 ? possibleDirections[0] : possibleDirections[1];
            }
            else
            {
                if (possibleDirections.Contains(preferredDirection))
                {
                    possibleDirections.Remove(preferredDirection);
                    possibleDirections.Remove(currentDirection);
                    currentDirection = selectedPercentage < 20 ? preferredDirection : selectedPercentage < 90 ? currentDirection : possibleDirections[0];
                }
                else
                {
                    possibleDirections.Remove(currentDirection);
                    currentDirection = selectedPercentage < 80 ? currentDirection : selectedPercentage < 90 ? possibleDirections[0] : possibleDirections[1];
                }
            }

            // Move position
            switch (currentDirection)
            {
                case Direction.North:
                    currentPosition.Y -= 1;
                    break;
                case Direction.East:
                    currentPosition.X += 1;
                    break;
                case Direction.South:
                    currentPosition.Y += 1;
                    break;
                case Direction.West:
                    currentPosition.X -= 1;
                    break;
            }

            // Add path tile and add tracks
            var existingTile = Grid[currentPosition.Y][currentPosition.X];
            if (existingTile is DirtTile)
            {
                var newTile = new PathTile();
                if (includeTracks)
                {
                    newTile.TrackType = TrackType.Intersection;
                    newTile.TrackDirection = currentDirection;
                }
                Grid[currentPosition.Y][currentPosition.X] = newTile;
            }
            else if (includeTracks && existingTile is PathTile pathTile)
            {
                pathTile.TrackType = TrackType.Intersection;
                pathTile.TrackDirection = currentDirection;
            }
        }
    }

    private static Direction GetPreferredDirection(int diffX, int diffY)
    {
        if (Math.Abs(diffX) > Math.Abs(diffY))
        {
            return diffX < 0 ? Direction.West : Direction.East;
        }
        return diffY < 0 ? Direction.North : Direction.South;
    }

    private void FixAllTracks()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = Grid[y][x];
                if (tile is not PathTile { TrackType: not null } pathTile) continue;
                
                var trackNorth = y != 0 && Grid[y-1][x] is PathTile { TrackType: not null };
                var trackEast = x != Width-1 && Grid[y][x+1] is PathTile { TrackType: not null };
                var trackSouth = y != Height-1 && Grid[y+1][x] is PathTile { TrackType: not null };
                var trackWest = x != 0 && Grid[y][x-1] is PathTile { TrackType: not null };

                var directions = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                var connections = new List<bool> { trackNorth, trackEast, trackSouth, trackWest };
                var amountOfConnections = connections.Select(v => v).Count();

                switch (amountOfConnections)
                {
                    case 0:
                    case 4:
                        pathTile.TrackType = TrackType.Intersection;
                        pathTile.TrackDirection = Direction.North;
                        break;
                    case 1:
                        pathTile.TrackType = TrackType.End;
                        pathTile.TrackDirection = directions[connections.IndexOf(true)];
                        break;
                    case 2:
                        pathTile.TrackDirection = directions[connections.IndexOf(true)];
                        if ((connections[0] && connections[2]) || (connections[1] && connections[3]))
                        {
                            pathTile.TrackType = TrackType.Straight;
                            break;
                        }

                        pathTile.TrackType = TrackType.Curved;
                        if (connections[0] && connections[3]) pathTile.TrackDirection = Direction.West;
                        
                        break;
                    case 3:
                        pathTile.TrackType = TrackType.Switch;
                        pathTile.TrackDirection = directions[connections.IndexOf(false)];
                        break;
                }
            }
        }
    }
    
    public void GenerateOres()
    {
        var random = new Random(Seed);
        
    }
}
