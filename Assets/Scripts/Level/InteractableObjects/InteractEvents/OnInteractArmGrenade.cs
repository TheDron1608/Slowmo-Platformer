using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class OnInteractArmGrenade : Interactable
{
    public float ExplodeDelay = 3f;
    public bool ArmOnStart = false;

    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite _armedSprite;
    [SerializeField] private Material _idleMaterial;
    [SerializeField] private Material _armedMaterial;
    [SerializeField] private ParticleSpawner _pinParticleSpawner;
    [SerializeField] private SoundPlayer _soundOnArm;

    private Coroutine _explodeCoroutine = null;
    private bool _armed = false;

    public bool Armed
    {
        get => _armed;
        set
        {
            if (_armed ==  value) return;

            if (value)
            {
                _pinParticleSpawner.SpawnParticle();
                _soundOnArm.PlaySound();
                GetComponent<SpriteRenderer>().sprite = _armedSprite;
                GetComponent<DynamicMaterial>().DefaultMaterial = _armedMaterial;

                if (_explodeCoroutine == null)
                {
                    _explodeCoroutine = StartCoroutine(AwaitDelayThenExplode());
                }
            }
            else
            {
                if (_explodeCoroutine != null)
                {
                    StopCoroutine(_explodeCoroutine);
                    _explodeCoroutine = null;
                }

                GetComponent<SpriteRenderer>().sprite = _idleSprite;
                GetComponent<DynamicMaterial>().DefaultMaterial = _idleMaterial;
            }

            _armed = value;
        }
    }

    private void Start()
    {
        if (ArmOnStart)
        {
            Armed = true;
        }
    }

    private void OnEnable()
    {
        _explodeCoroutine = null;
        if (_armed) _explodeCoroutine = StartCoroutine(AwaitDelayThenExplode());
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return base.StartInteractCondition(interactor) && _explodeCoroutine == null;
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        Armed = true;
    }

    private IEnumerator AwaitDelayThenExplode()
    {
        yield return new WaitForSeconds(ExplodeDelay);

        GetComponent<BreakableObject>().BreakObject(null);
        _explodeCoroutine = null;
    }

    public override void InvokeOnEffectApllied(AbstractEffect effect, ObjectEffectsReceiver receiver, List<IEffectApplier> appliers)
    {
        if (TryGetComponent(out BreakableObject breakable) && breakable.Breaker != null && breakable.Breaker.TryGetComponent(out IEffectApplier breakEffectApplier))
        {
            appliers.Add(breakEffectApplier);
        }

        base.InvokeOnEffectApllied(effect, receiver, appliers);

        if (TryGetComponent(out Holdable holdable) && holdable.CurrentOrLastHolder != null && !holdable.CurrentOrLastHolder.IsDestroyed())
        {
            holdable.CurrentOrLastHolder.CharComponents.CharacterAttacking.InvokeOnEffectApllied(effect, receiver, appliers);
        }
    }
}