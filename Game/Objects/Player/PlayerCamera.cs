using Godot;

namespace Spaghetti;

public partial class PlayerCamera : Camera2D
{
    [Export] public Player Player { get; set; } = null!;
    [Export] public float TransitionTime { get; set; } = 1f;

    public Section? Section
    {
        get => m_Section;
        set
        {
            if (Section != null)
            {
                Section.Active = false;
            }

            if (value == null)
            {
                m_Section = null;
                return;
            }

            m_Section = value;
            m_Section.Active = true;

            LimitLeft = m_Section.LimitLeft + (int)m_Section.Position.X;
            LimitRight = m_Section.LimitRight + (int)m_Section.Position.X;
            LimitTop = m_Section.LimitTop + (int)m_Section.Position.Y;
            LimitBottom = m_Section.LimitBottom + (int)m_Section.Position.Y;
        }
    }

    private Section? m_Section;

    public override void _Ready()
    {
        GlobalPosition = GetPlayerCenter();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = GetPlayerCenter();
    }

    private Vector2 GetPlayerCenter()
    {
        return Player.GlobalPosition;
    }
}
