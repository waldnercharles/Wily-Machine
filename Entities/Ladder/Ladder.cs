using System;
using System.Collections.Generic;
using Godot;

namespace Spaghetti.Godot;

public partial class Ladder : StaticBody2D
{
    private const int TileSize = 16;

    [Export] public CollisionShape2D LadderTopCollision { get; set; } = null!;
    [Export] public Area2D LadderArea { get; set; } = null!;
    [Export] public CollisionShape2D LadderCollision { get; set; } = null!;

    private int m_SizeInTiles;

    [Export] public int SizeInTiles
    {
        get => m_SizeInTiles;
        set
        {
            m_SizeInTiles = value;

            var rectangleShape = LadderCollision.Shape as RectangleShape2D ??
                                 throw new Exception(
                                     "Ladder collision shape must be a RectangleShape2D");

            rectangleShape.Size =
                new Vector2(rectangleShape.Size.X, SizeInTiles * TileSize / 2f);

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
            player.MoveAndCollide(
                new Vector2(0, -GetDistanceToLadderTop(player) - 1));
        }

        player.StopClimbing();
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

                player.StartClimbing(distanceToCenter);
                SetCollidable(player, false);
            }
        }
    }

    public bool IsExitingLadder(Player player)
    {
        return m_Players.Contains(player) &&
               GetDistanceToLadderTop(player) < player.CollisionShape.Shape.GetRect().Size.Y + 2;
    }

    private void SetCollidable(Player player, bool value)
    {
        player.SetCollisionMaskValue(6, value);
    }

    private bool IsAboveLadder(Player player)
    {
        return GetDistanceToLadderTop(player) < 0;
    }

    private float GetDistanceToLadderTop(Player player)
    {
        return player.CollisionShape.GlobalPosition.Y - LadderTopCollision.GlobalPosition.Y +
               player.CollisionShape.Shape.GetRect().Size.Y;
    }
}
