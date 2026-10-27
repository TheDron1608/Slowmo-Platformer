
public class MeatHook : MeleeWeapon
{
    const string ANIMATOR_NO_EXTRA_HOOK_PROP_NAME = "NoExtraHook";

    public int DefaultRepeatAttackTimes;
    public int NoExtraHookRepeatAttackTimes;

    private bool _noExtraHook = false;

    public bool NoExtraHook
    {
        get => _noExtraHook;
        set
        {
            if (_noExtraHook == value) return;
            _noExtraHook = value;

            RepeatAttacksTimes = value ? NoExtraHookRepeatAttackTimes : DefaultRepeatAttackTimes;
            _animator.SetBool(ANIMATOR_NO_EXTRA_HOOK_PROP_NAME, value);
        }
    }
}