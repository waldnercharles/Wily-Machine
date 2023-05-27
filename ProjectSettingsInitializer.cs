﻿using System.Text;
using Godot;
using Godot.Collections;

namespace Spaghetti.Godot;

public static class ProjectSettingsPath
{
    public const string InitialScene = "custom/startup/initial_scene";
}

[Tool]
public partial class ProjectSettingsInitializer : Node
{
    public override void _Ready()
    {
        AddProjectSetting(ProjectSettingsPath.InitialScene, "res://Stages/Level.tscn", Variant.Type.String, PropertyHint.File);
    }

    private static void AddProjectSetting(string path, Variant value, Variant.Type type, PropertyHint hint = PropertyHint.None, string hintString = "")
    {
        if (ProjectSettings.HasSetting(path))
        {
            return;
        }

        ProjectSettings.SetSetting(ProjectSettingsPath.InitialScene, "res://Stages/Level.tscn");
        ProjectSettings.SetInitialValue(ProjectSettingsPath.InitialScene, "res://Stages/Level.tscn");

        var propertyInfo = new Dictionary
        {
            { "name", path },
            { "type", (long)type },
            { "hint", (long)hint },
            { "hint_string", hintString }
        };

        ProjectSettings.AddPropertyInfo(propertyInfo);
    }
}
