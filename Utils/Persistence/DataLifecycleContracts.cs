namespace STS2RitsuLib.Utils.Persistence
{
    public enum DataLifecycleState
    {
        WaitingForProfile = 0,
        Ready = 1,
    }

    public readonly record struct ProfileDataReadyEvent(
        int ProfileId,
        string Source,
        bool IsInitialReady,
        bool IsProfileSwitch,
        bool DataReloaded,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileDataChangedEvent(
        int OldProfileId,
        int NewProfileId,
        string Source,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;

    public readonly record struct ProfileDataInvalidatedEvent(
        int ProfileId,
        string Reason,
        DateTimeOffset OccurredAtUtc
    ) : IFrameworkLifecycleEvent;
}
