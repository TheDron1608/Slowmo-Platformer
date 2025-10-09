using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Animator))]
public class AutoDisableAnimatorsOnDistance : MonoBehaviour
{
    const float DISTANCE_TO_DISABLE = 50f;

    private Animator _animatorComponent;

    private void Awake()
    {
        _animatorComponent = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        _animatorComponent.enabled = Vector2.Distance(Camera.main.transform.position, transform.position) < DISTANCE_TO_DISABLE;
    }
}
