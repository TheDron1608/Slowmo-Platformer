
using Unity.VisualScripting;
using UnityEngine;

public class GiveSpecial : AbstractCharacterEffect, IEntireCharacterEffect
{
    public AbstractCharacterSpecial Speical;

    private AbstractCharacterSpecial _oldSpecial;
    private AbstractCharacterSpecial _addedSpecial;

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<GiveSpecial>();
    }

    protected override void OnApply()
    {
        base.OnApply();

        _oldSpecial = AffectedCharacter.CharacterSpecial;
        _addedSpecial = Instantiate(Speical, AffectedCharacter.transform);
        AffectedCharacter.CharacterSpecial = _addedSpecial;
        if (_oldSpecial != null && !_oldSpecial.IsDestroyed()) _oldSpecial.gameObject.SetActive(false);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (_addedSpecial != null && !_addedSpecial.IsDestroyed()) Destroy(_addedSpecial.gameObject);
        if (_oldSpecial != null && !_oldSpecial.IsDestroyed())
        {
            _oldSpecial.gameObject.SetActive(true);
            AffectedCharacter.CharacterSpecial = _oldSpecial;
        }
    }
}