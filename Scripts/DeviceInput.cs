using Godot;

namespace Spaghetti.Godot;

public partial class DeviceInput : RefCounted
{
    [Signal] public delegate void ConnectionChangedEventHandler(bool connected);

    public int Device;
    public bool IsDeviceConnected = true;

    public DeviceInput(int device)
    {
        Device = device;
        Input.JoyConnectionChanged += JoyConnectionChanged;
    }

    public bool IsKeyboard() => Device < 0;
    public bool IsJoypad() => Device >= 0;

    public bool IsKnown() => IsKeyboard() || Input.IsJoyKnown(Device);

    public string GetGuid() => IsKeyboard() ? "Keyboard" : Input.GetJoyGuid(Device);
    public string GetName() => IsKeyboard() ? "Keyboard" : Input.GetJoyName(Device);

    public float GetVibrationDuration() => IsKeyboard() ? 0f : Input.GetJoyVibrationDuration(Device);
    public Vector2 GetVibrationStrength() => IsKeyboard() ? Vector2.Zero : Input.GetJoyVibrationStrength(Device);

    public string GetDeviceAction(string action) => action;

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

    public Vector2 GetVector(string negativeX, string positiveX, string negativeY, string positiveY, float deadzone = -1f)
    {
        return IsDeviceConnected ? Input.GetVector(negativeX, positiveX, negativeY, positiveY, deadzone) : Vector2.Zero;
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

    private void JoyConnectionChanged(long device, bool connected)
    {
        if (device == Device)
        {
            IsDeviceConnected = connected;
            EmitSignal(SignalName.ConnectionChanged, connected);
        }
    }
}
