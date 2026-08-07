using FMOD.Studio;
using FMODUnity;

public static class AudioTransition
{
    public const string DiageticBusPath = "bus:/diagetic_ALL";

    public static bool TryGetDiageticBus(out Bus bus)
    {
        bus = default;

        return RuntimeManager.IsInitialized
               && RuntimeManager.StudioSystem.getBus(DiageticBusPath, out bus) == FMOD.RESULT.OK
               && bus.isValid();
    }

    // Silence the world before a scene goes away. The commands are flushed on the spot:
    // FMOD queues them for its own update, so without the flush the stop can land after the
    // next scene's emitters have already started and kill them instead.
    public static void Silence(bool cutNow)
    {
        if (AudioSnapshotManager.Instance)
        {
            AudioSnapshotManager.Instance.Clear();
        }

        if (!TryGetDiageticBus(out Bus bus))
        {
            DevLog.Warn($"{nameof(AudioTransition)}: no '{DiageticBusPath}' bus yet, leaving audio as it is");
            return;
        }

        bus.setMute(true);
        bus.stopAllEvents(cutNow ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        if (cutNow)
        {
            RuntimeManager.StudioSystem.flushCommands();
        }
    }

    public static void SetDiageticVolume(float volume)
    {
        if (!TryGetDiageticBus(out Bus bus)) return;

        bus.setVolume(volume);
        bus.setMute(false);
    }

    // a mute is global and outlives the scene that set it, so every arrival lifts it again -
    // a scene without an FMODResetManager of its own would otherwise start deaf
    public static void Restore() => SetDiageticVolume(1f);
}
