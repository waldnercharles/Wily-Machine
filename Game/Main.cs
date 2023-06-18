using Godot;

namespace Spaghetti;

public partial class Main : Node
{
    [Export] public SubViewportContainer ViewportContainer { get; set; } = null!;
    [Export] public SubViewport Viewport { get; set; } = null!;

    private Node? m_Scene;
    private string? m_ScenePath;

    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(Colors.Black);

        ViewportContainer.SetProcessUnhandledInput(true);
        SwitchSceneDeferred(ProjectSettings.GetSetting(ProjectSettingsPath.InitialScene).ToString());
    }

    public void SwitchSceneDeferred(string path) => CallDeferred(nameof(SwitchScene), path);
    public void ReloadSceneDeferred() => CallDeferred(nameof(SwitchScene), m_ScenePath ?? string.Empty);

    public void SwitchScene(string? path)
    {
        m_Scene?.Free();

        if (!string.IsNullOrEmpty(path))
        {
            m_ScenePath = path;
            m_Scene = ResourceLoader.Load<PackedScene>(path).Instantiate();
            Viewport.AddChild(m_Scene);
        }

        m_ScenePath = path;
    }
}
