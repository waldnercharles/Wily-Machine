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

            LimitLeft = m_Section.LimitLeft;
            LimitRight = m_Section.LimitRight;
            LimitTop = m_Section.LimitTop;
            LimitBottom = m_Section.LimitBottom;
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
