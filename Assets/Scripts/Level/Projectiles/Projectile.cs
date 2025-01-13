using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Weapon Weapon;
    public Weapon.AttackPiercing Pierce = Weapon.AttackPiercing.NO_PIERCE;
    public float Damage = 1f;

    public void InitializeOwner(Weapon owner)
    {
        Weapon = owner;
        Pierce =  owner.Pierce;
        Damage = owner.Damage;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }
}
