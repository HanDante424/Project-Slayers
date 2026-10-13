public interface ICharacterMechanism
{
    bool IsActing { get; }
    void RequestBasicAttack();
    void CancelAction();
}
