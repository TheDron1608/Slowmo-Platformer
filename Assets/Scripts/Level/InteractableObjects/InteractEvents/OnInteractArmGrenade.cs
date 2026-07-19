using System.Collections;
using UnityEngine;

public class OnInteractArmGrenade : Interactable
{
    public float ExplodeDelay = 3f;

    [SerializeField] private Sprite _armedSprite;
    [SerializeField] private Material _armedMaterial;
    [SerializeField] private ParticleSpawner _pinParticleSpawner;
    [SerializeField] private SoundPlayer _soundOnArm;

    private Coroutine _explodeCoroutine = null;

    private void OnEnable()
    {
        _explodeCoroutine = null;
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return base.StartInteractCondition(interactor) && _explodeCoroutine == null;
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        _pinParticleSpawner.SpawnParticle();
        _soundOnArm.PlaySound();
        GetComponent<SpriteRenderer>().sprite = _armedSprite;
        GetComponent<DynamicMaterial>().DefaultMaterial = _armedMaterial;

        _explodeCoroutine = StartCoroutine(AwaitDelayThenExplode());
    }

    private IEnumerator AwaitDelayThenExplode()
    {
        yield return new WaitForSeconds(ExplodeDelay);

        GetComponent<BreakableObject>().BreakObject(null);
        _explodeCoroutine = null;
    }
}