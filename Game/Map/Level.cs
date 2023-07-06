using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WilyMachine;

[SceneTree]
public partial class Level : Node2D
{
    public PlayerCamera Camera => _.Camera;

    [Export] public PackedScene Spike { get; set; } = null!;
    [Export] public PackedScene Ladder { get; set; } = null!;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        CallDeferred(nameof(AddSpikes));
        CallDeferred(nameof(AddLadders));
    }

    private IEnumerable<Room> GetRooms() => GetChildren().OfType<Room>();

    private void AddSpikes()
    {
        var spikesNode = new Node2D { Name = "Spikes" };
        AddChild(spikesNode);

        foreach (var room in GetRooms())
        {
            var spikes = room.GetCellsWithCustomData("Type", "Spike").Select(room.MapToLocal).ToList();

            foreach (var spikePosition in spikes)
            {
                var spike = Spike.Instantiate<Node2D>();
                spike.Position = spikePosition;

                spikesNode.AddChild(spike);
            }
        }

        Log.Trace("Added {0} spikes", spikesNode.GetChildCount().ToString());
    }

    private void AddLadders()
    {
        var laddersNode = new Node2D { Name = "Ladders" };
        AddChild(laddersNode);

        var rooms = GetRooms().ToArray();

        var ladderPositions = rooms
            .SelectMany(room =>
                room.GetCellsWithCustomData("Type", "Ladder")
                    .Select(cell => room.GlobalPosition + room.MapToLocal(cell))).ToList();

        while (ladderPositions.Count > 0)
        {
            ladderPositions.Sort((a, b) =>
            {
                var xSort = a.X.CompareTo(b.X);
                return xSort == 0 ? a.Y.CompareTo(b.Y) : xSort;
            });

            var tileSize = rooms.First().TileSet.TileSize;
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
            ladder.GlobalPosition = ladderPosition;
            ladder.SizeInTiles = ladderHeight;

            laddersNode.AddChild(ladder);
        }

        Log.Trace("Added {0} ladders", laddersNode.GetChildCount().ToString());
    }
}
