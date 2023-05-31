using System.Collections.Generic;
using Godot;

namespace Spaghetti.Godot;

public partial class Ladder : StaticBody2D
{
    [Export] public int TileSize { get; set; } = 16;

    [Export] public CollisionShape2D LadderTopCollision { get; set; } = null!;

    [Export] public Area2D LadderArea { get; set; } = null!;
    [Export] public CollisionShape2D LadderCollision { get; set; } = null!;

    private int m_SizeInTiles = 3;

    public int SizeInTiles
    {
        get => m_SizeInTiles;
        set
        {
            m_SizeInTiles = value;

            var ladderCollisionShape = LadderCollision.Shape as RectangleShape2D;
            Log.Assert(ladderCollisionShape != null);

            ladderCollisionShape.Size =
                new Vector2(ladderCollisionShape.Size.X, SizeInTiles * TileSize);

            LadderCollision.Position =
                new Vector2(LadderCollision.Position.X, SizeInTiles * TileSize / 2f - 1);
        }
    }

    private readonly HashSet<Player> m_Players = new HashSet<Player>();

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        LadderArea.BodyEntered += BodyEntered;
        LadderArea.BodyExited += BodyExited;
    }

    private void BodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            m_Players.Add(player);
            player.Ladder = this;

            SetPhysicsProcess(true);
        }
    }

    private void BodyExited(Node2D body)
    {
        if (body is not Player player || !m_Players.Contains(player))
        {
            return;
        }

        if (IsAboveLadder(player) && player.IsClimbing)
        {
            player.MoveAndCollide(new Vector2(0, -GetDistanceToLadderTop(player)));
        }

        if (player.IsClimbing)
        {
            player.StateMachine.ChangeState<PlayerMoveState>();
        }

        player.Ladder = null;
        SetCollidable(player, true);
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
            if (!player.IsClimbing && !player.IsSliding && !player.IsStaggered &&
                (player.Input.IsActionPressed(PlayerInputAction.Up) && !IsAboveLadder(player) ||
                 player.Input.IsActionPressed(PlayerInputAction.Down) && IsAboveLadder(player)))
            {
                var distanceToCenter = LadderCollision.GlobalPosition - player.GlobalPosition;
                distanceToCenter.Y = 0;

                player.MoveAndCollide(distanceToCenter);
                player.StateMachine.ChangeState<PlayerClimbState>();
                SetCollidable(player, false);
            }
        }
    }

    public bool IsExitingLadder(Player player)
    {
        return m_Players.Contains(player) &&
               GetDistanceToLadderTop(player) <
               player.CollisionShape.Shape.GetRect().Size.Y / 2f + 2;
    }

    public bool IsAboveLadder(Player player)
    {
        return GetDistanceToLadderTop(player) <= 0;
    }

    public float GetDistanceToLadderTop(Player player)
    {
        return player.CollisionShape.GlobalPosition.Y +
               player.CollisionShape.Shape.GetRect().Size.Y / 2f -
               LadderTopCollision.GlobalPosition.Y - LadderTopCollision.Shape.GetRect().Size.Y / 2f;
    }

    private void SetCollidable(Player player, bool value)
    {
        const int ladderMaskValue = 7;
        player.SetCollisionMaskValue(ladderMaskValue, value);
    }
}
