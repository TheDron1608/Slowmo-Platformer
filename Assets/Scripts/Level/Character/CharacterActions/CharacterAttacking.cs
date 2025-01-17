using UnityEngine;

public class CharacterAttacking : MonoBehaviour
{
    public bool IsAbleToAttack = true;

    private bool _autoAttack = false;
    private Vector2 _autoAttackDirection = Vector2.right;

    private CharacterHoldingObjects _characterHoldingObjects;

    public bool AutoAttack
    {
        get => _autoAttack;
        set => _autoAttack = value;
    }

    public Vector2 AutoAttackDirection
    {
        get => _autoAttackDirection;
        set => _autoAttackDirection = value;
    }

    private void Awake()
    {
        _characterHoldingObjects = GetComponent<CharacterHoldingObjects>();
    }

    public void Attack(Vector2 direction)
    {
        if (_characterHoldingObjects != null && _characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Weapon weapon))
        {
            weapon.TryAttack(direction);
        }
    }

    private void FixedUpdate()
    {
        if (AutoAttack)
        {
            Attack(AutoAttackDirection);
        }
    }
}
