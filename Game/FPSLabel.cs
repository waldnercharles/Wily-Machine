using Godot;
using System;

public partial class FPSLabel : Label
{
    public double AverageFPS = 60;

    public override void _Process(double delta)
    {
        AverageFPS = (AverageFPS + 1f / delta) / 2f;
        Text = Mathf.RoundToInt(AverageFPS).ToString();
    }
}
