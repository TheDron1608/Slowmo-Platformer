using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Weapon Weapon;
    public Weapon.AttackPiercing Pierce = Weapon.AttackPiercing.NO_PIERCE;
    public float Damage = 1f;

    private void Awake()
    {
        LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForGameObject(gameObject);
    }

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("enter");
        Remove();
    }
}
