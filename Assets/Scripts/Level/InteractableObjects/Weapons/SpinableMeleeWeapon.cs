using UnityEngine;

public class SpinableMeleeWeapon : MeleeWeapon
{
    const string ANIMATOR_SPIN_TRIGGER_NAME = "Spin";

    public void Spin()
    {
        _animator.SetTrigger(ANIMATOR_SPIN_TRIGGER_NAME);
    }
}
