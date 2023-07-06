using Godot;

namespace WilyMachine;

[SceneTree] [Tool]
public partial class ConstantForce : Area2D
{
    [Export] public bool Enabled { get; set; } = true;
    [Export] public Vector2 Velocity { get; set; }

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Actor actor)
        {
            actor.AddConstanstForce(this);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Actor actor)
        {
            actor.RemoveConstantForce(this);
        }
    }
}
