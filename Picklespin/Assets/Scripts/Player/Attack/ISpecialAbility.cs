public interface ISpecialAbility
{
    SpecialAbilityId Id { get; }
    bool IsUsable { get; }
    bool TryUse();
    bool CancelUse();
}
