using UnityEngine;

public class CharacterHitbox : AbstractCharacterComponent
{
    public bool HitableByProjectiles = true;
    /// <summary>
    /// If projectile hits two multiple parts of a single character same time, hit detection will be triggered on hitbox with the highest HitPriority
    /// </summary>
    public int HitPriority = 1;

    public virtual void OnHit()
    {
        Debug.Log("hit: " + gameObject.name);
    }
}
