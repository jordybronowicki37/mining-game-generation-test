using System.Drawing;

namespace MiningGameMapGenerationTest.MapGeneration.Tiles;

public class BaseTile: Tile
{
    public BaseType BaseType { get; init; } = BaseType.General;
}