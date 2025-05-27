
using UnityEngine;

interface IDamagable
{
    public void ApplyDamage(float damage, MonoBehaviour damager);
    public bool PiercableThrought { get; set; }
}