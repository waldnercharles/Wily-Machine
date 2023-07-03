using System.Collections.Generic;
using Godot;

namespace WilyMachine;

[SceneTree]
public partial class Ladder : StaticBody2D
{
    public CollisionShape2D TopCollisionShape => _.TopCollisionShape;

    public Area2D Area => _.Area;
    public CollisionShape2D CollisionShape => _.Area.CollisionShape2D;

    [Export] public int TileSize { get; set; } = 16;

    private int m_SizeInTiles = 1;

    public int SizeInTiles
    {
        get => m_SizeInTiles;
        set
        {
            m_SizeInTiles = value;

            var ladderCollisionShape = CollisionShape.Shape as RectangleShape2D;
            Log.Assert(ladderCollisionShape != null);

            ladderCollisionShape.Size =
                new Vector2(ladderCollisionShape.Size.X, SizeInTiles * TileSize);

            CollisionShape.Position =
                new Vector2(CollisionShape.Position.X, SizeInTiles * TileSize / 2f - 1);
        }
    }

    private readonly HashSet<Player> m_Players = new HashSet<Player>();

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        Area.BodyEntered += OnBodyEntered;
        Area.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.Ladder = this;
            player.IsTouchingLadder = true;
            m_Players.Add(player);

            SetPhysicsProcess(true);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is not Player player || !m_Players.Contains(player))
        {
            return;
        }

        player.Ladder = null;
        player.IsTouchingLadder = false;
        m_Players.Remove(player);

        if (m_Players.Count == 0)
        {
            SetPhysicsProcess(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var player in m_Players)
        {
            if (player.IsClimbing || player.IsStunned)
            {
                continue;
            }

            var controller = player.Controller;

            if (player.CurrentCollisionShape != null)
            {
                var playerFeet = player.CurrentCollisionShape.GlobalPosition.Y +
                                 player.CurrentCollisionShape.Shape.GetRect().End.Y;

                var ladderTop = TopCollisionShape.GlobalPosition.Y +
                                TopCollisionShape.Shape.GetRect().Position.Y;

                var distanceToLadderTop = playerFeet - ladderTop;
                var isAboveLadder = distanceToLadderTop <= 0;

                if (controller.ShouldMoveUp() && !isAboveLadder || controller.ShouldMoveDown() && isAboveLadder)
                {
                    player.SetMovementState<ClimbMovementState>();
                }
            }
        }
    }
}
