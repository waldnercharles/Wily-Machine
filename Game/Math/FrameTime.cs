namespace WilyMachine;

public static class FrameTime
{
    public static ulong ToMilliseconds(int frames) => (ulong)(frames * 1000.0 / 60.0);
}
