using Godot;

namespace Spaghetti;

public static class Global
{
    public static Vector2I Resolution { get; set; } = new Vector2I(1280, 720);
    public static Vector2I InternalResolution => new Vector2I(400, 240);
}

[SceneTree]
public partial class Main : Node
{
    public SubViewportContainer SubViewportContainer => _.SubViewportContainer;
    public SubViewport SubViewport => _.SubViewportContainer.SubViewport;

    private Node? m_Scene;
    private string? m_ScenePath;

    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(Colors.Black);

        SubViewportContainer.Stretch = true;
        SubViewportContainer.SetProcessUnhandledInput(true);

        SubViewport.RenderTargetClearMode = SubViewport.ClearMode.Always;
        SubViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;

        SubViewport.Size = Global.Resolution;
        SubViewport.Size2DOverride = Global.InternalResolution;

        SubViewport.HandleInputLocally = false;

        SubViewport.CanvasItemDefaultTextureFilter = Godot.Viewport.DefaultCanvasItemTextureFilter.Nearest;
        // AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);

        SubViewport.AudioListenerEnable2D = true;

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
            SubViewport.AddChild(m_Scene);
        }

        m_ScenePath = path;
    }
}
