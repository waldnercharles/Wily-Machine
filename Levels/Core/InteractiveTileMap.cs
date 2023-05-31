using System.Collections.Generic;
using Godot;

namespace Spaghetti.Godot;

public partial class InteractiveTileMap : TileMap
{
    private PackedScene m_Ladder =>
        ResourceLoader.Load<PackedScene>("res://Entities/Ladder/Ladder.tscn");

    public override void _Ready()
    {
        CallDeferred(nameof(AddLadders));
    }

    private void AddLadders()
    {
        var laddersNode = new Node2D
        {
            Name = "Ladders"
        };

        AddChild(laddersNode);

        var ladders = new List<Vector2>();

        for (var layer = 0; layer < GetLayersCount(); layer++)
        {
            foreach (var cell in GetUsedCells(layer))
            {
                var cellData = GetCellTileData(layer, cell);
                var cellType = cellData.GetCustomData("Type").ToString();

                switch (cellType)
                {
                    case "Ladder":
                    {
                        var localPosition = MapToLocal(cell);
                        var globalPosition = ToGlobal(localPosition);

                        ladders.Add(localPosition);
                        break;
                    }
                }
            }
        }

        while (ladders.Count > 0)
        {
            ladders.Sort((a, b) =>
            {
                var xSort = a.X.CompareTo(b.X);
                return xSort == 0 ? a.Y.CompareTo(b.Y) : xSort;
            });

            var tileSize = TileSet.TileSize;
            var ladderPosition = ladders[0] - new Vector2(0, tileSize.Y) / 2f;

            var ladderCount = 1;

            while (ladders.Count > 1)
            {
                if (ladders[0].X == ladders[1].X && ladders[1].Y == ladders[0].Y + tileSize.Y)
                {
                    ladderCount++;
                    ladders.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            ladders.RemoveAt(0);

            var ladder = m_Ladder.Instantiate<Ladder>();
            ladder.Position = ladderPosition;
            ladder.SizeInTiles = ladderCount;

            laddersNode.AddChild(ladder);
        }
    }
}
