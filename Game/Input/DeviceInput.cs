using System.Collections.Generic;
using Godot;

namespace Spaghetti;

public partial class DeviceInput : Node
{
    [Signal] public delegate void ConnectionChangedEventHandler(bool connected);

    public int Device { get; set; } = 0;
    public bool IsDeviceConnected = true;

    private static readonly Dictionary<(int, long), ulong> s_Timestamps;

    static DeviceInput()
    {
        s_Timestamps = new Dictionary<(int, long), ulong>();
    }

    public override void _Ready()
    {
        Input.JoyConnectionChanged += JoyConnectionChanged;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsPressed() || e.IsEcho())
        {
            return;
        }

        if (e is InputEventKey keyEvent)
        {
            s_Timestamps[(e.Device, (long)keyEvent.PhysicalKeycode)] = Time.GetTicksMsec();
        }
        else if (e is InputEventJoypadButton joypadEvent)
        {
            s_Timestamps[(e.Device, (long)joypadEvent.ButtonIndex)] = Time.GetTicksMsec();
        }
    }

    public bool IsKeyboard()
    {
        return Device == 0;
    }

    public bool IsJoypad()
    {
        return Device > 0;
    }

    public bool IsKnown()
    {
        return IsKeyboard() || Input.IsJoyKnown(Device);
    }

    public string? GetGuid()
    {
        return IsKeyboard() ? "Keyboard" : Input.GetJoyGuid(Device);
    }

    public string? GetName()
    {
        return IsKeyboard() ? "Keyboard" : Input.GetJoyName(Device);
    }

    public float GetVibrationDuration()
    {
        return IsKeyboard() ? 0f : Input.GetJoyVibrationDuration(Device);
    }

    public Vector2 GetVibrationStrength()
    {
        return IsKeyboard() ? Vector2.Zero : Input.GetJoyVibrationStrength(Device);
    }

    public string GetDeviceAction(string action)
    {
        return action;
    }

    public void StartVibration(float weakMagnitude, float strongMagnitude, float duration = 0f)
    {
        if (!IsKeyboard())
        {
            Input.StartJoyVibration(Device, weakMagnitude, strongMagnitude, duration);
        }
    }

    public void StopVibration()
    {
        if (!IsKeyboard())
        {
            Input.StopJoyVibration(Device);
        }
    }

    // TODO: Actually make this work for multiplayer
    public float GetActionRawStrength(string action, bool exactMatch = false)
    {
        return IsDeviceConnected ? Input.GetActionRawStrength(action, exactMatch) : 0f;
    }

    public float GetActionStrength(string action, bool exactMatch = false)
    {
        return IsDeviceConnected ? Input.GetActionStrength(action, exactMatch) : 0f;
    }

    public float GetAxis(string negativeAction, string positiveAction)
    {
        return IsDeviceConnected ? Input.GetAxis(negativeAction, positiveAction) : 0f;
    }

    public Vector2 GetVector(string negativeX, string positiveX, string negativeY, string positiveY,
        float deadzone = -1f)
    {
        return IsDeviceConnected
            ? Input.GetVector(negativeX, positiveX, negativeY, positiveY, deadzone)
            : Vector2.Zero;
    }

    public bool IsActionJustPressed(string action, bool exactMatch = false)
    {
        return IsDeviceConnected && Input.IsActionJustPressed(action, exactMatch);
    }

    public bool IsActionJustReleased(string action, bool exactMatch = false)
    {
        return IsDeviceConnected && Input.IsActionJustReleased(action, exactMatch);
    }

    public bool IsActionPressed(string action, bool exactMatch = false)
    {
        return IsDeviceConnected && Input.IsActionPressed(action, exactMatch);
    }

    public bool IsActionBuffered(string action, ulong bufferMilliseconds)
    {
        bool IsBuffered(long button)
        {
            if (s_Timestamps.TryGetValue((Device, button), out var timestamp))
            {
                return Time.GetTicksMsec() - timestamp < bufferMilliseconds;
            }

            return false;
        }

        var actions = InputMap.ActionGetEvents(action);

        for (var i = 0; i < actions.Count; i++)
        {
            var inputEvent = actions[i];

            if (inputEvent is InputEventKey keyEvent && IsBuffered((long)keyEvent.PhysicalKeycode))
            {
                return true;
            }

            if (inputEvent is InputEventJoypadButton joypadEvent && IsBuffered((long)joypadEvent.ButtonIndex))
            {
                return true;
            }
        }

        return false;
    }

    public void ResetActionBuffer(string action)
    {
        var actions = InputMap.ActionGetEvents(action);

        for (var i = 0; i < actions.Count; i++)
        {
            var inputEvent = actions[i];

            if (inputEvent is InputEventKey keyEvent)
            {
                s_Timestamps[(Device, (long)keyEvent.PhysicalKeycode)] = 0;
            }
            else if (inputEvent is InputEventJoypadButton joypadEvent)
            {
                s_Timestamps[(Device, (long)joypadEvent.ButtonIndex)] = 0;
            }
        }
    }

    public bool IsActionHeld(string action)
    {
        return Input.IsActionPressed(action) && !Input.IsActionJustPressed(action);
    }

    private void JoyConnectionChanged(long device, bool connected)
    {
        if (device == Device)
        {
            IsDeviceConnected = connected;
            EmitSignal(DeviceInput.SignalName.ConnectionChanged!, connected);
        }
    }
}
