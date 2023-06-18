using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Spaghetti;

public partial class InteractiveTileMap : TileMap
{
    [Export] public PackedScene Spike { get; set; } = null!;
    [Export] public PackedScene Ladder { get; set; } = null!;

    public override void _Ready()
    {
        CallDeferred(nameof(AddSpikes));
        CallDeferred(nameof(AddLadders));
    }

    private void AddSpikes()
    {
        var spikesNode = new Node2D { Name = "Spikes" };
        AddChild(spikesNode);

        var spikes = GetCellsWithType("Spike").Select(MapToLocal).ToList();

        foreach (var spikePosition in spikes)
        {
            var spike = Spike.Instantiate<Node2D>();
            spike.Position = spikePosition;

            spikesNode.AddChild(spike);
        }

        Log.Trace("Added {0} spikes", spikesNode.GetChildCount().ToString());
    }

    private void AddLadders()
    {
        var laddersNode = new Node2D { Name = "Ladders" };
        AddChild(laddersNode);

        var ladderPositions = GetCellsWithType("Ladder").Select(MapToLocal).ToList();

        while (ladderPositions.Count > 0)
        {
            ladderPositions.Sort((a, b) =>
            {
                var xSort = a.X.CompareTo(b.X);
                return xSort == 0 ? a.Y.CompareTo(b.Y) : xSort;
            });

            var tileSize = TileSet.TileSize;
            var ladderPosition = ladderPositions[0] - new Vector2(0, tileSize.Y) / 2f;

            var ladderHeight = 1;

            while (ladderPositions.Count > 1)
            {
                if (ladderPositions[0].X == ladderPositions[1].X &&
                    ladderPositions[1].Y == ladderPositions[0].Y + tileSize.Y)
                {
                    ladderHeight++;
                    ladderPositions.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            ladderPositions.RemoveAt(0);

            var ladder = Ladder.Instantiate<Ladder>();
            ladder.Position = ladderPosition;
            ladder.SizeInTiles = ladderHeight;

            laddersNode.AddChild(ladder);
        }

        Log.Trace("Added {0} ladders", laddersNode.GetChildCount().ToString());
    }

    private IEnumerable<Vector2I> GetCellsWithType(string type)
    {
        for (var layer = 0; layer < GetLayersCount(); layer++)
        {
            foreach (var cell in GetUsedCells(layer))
            {
                var cellData = GetCellTileData(layer, cell);
                var cellType = cellData?.GetCustomData("Type").ToString();

                if (cellType == type)
                {
                    yield return cell;
                }
            }
        }
    }
}
