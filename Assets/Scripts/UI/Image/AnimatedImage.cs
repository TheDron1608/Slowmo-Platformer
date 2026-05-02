using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedImage : MonoBehaviour
{
    public enum AnimatedSpritePlayMode
    {
        LOOP,
        ONESHOT_HIDE_ON_FINISH,
        ONESHOT_DESTROY_ON_FINISH,
        ONESHOT_KEEP_ON_FINISH
    }

    [SerializeField]
    private List<Sprite> _frames = new List<Sprite>();

    public int FPS;
    public AnimatedSpritePlayMode AnimationPlayMode;
    public bool IsPlaying = true;
    public bool ReversePlaying = false;
    public bool TimeScaleDepended = true;

    private Image _imageComponent;
    private int _currentFrame;
    private float _timeLeftSinceLastFrame = 0f;
    private Coroutine _updateCoroutine = null;

    public event EventHandler AnimationFinished;

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            _currentFrame = value;
            if (_currentFrame >= _frames.Count)
            {
                _currentFrame = 0;
            }
            else if (_currentFrame < 0)
            {
                _currentFrame = _frames.Count - 1;
            }
            UpdateFrame();
        }
    }

    private void Awake()
    {
        if (ReversePlaying)
        {
            _currentFrame = _frames.Count - 1;
        }
        else
        {
            _currentFrame = 0;
        }

        if (!TryGetComponent<Image>(out _imageComponent))
        {
            throw new UnityException("image component not found");
        }
    }

    private void OnEnable()
    {
        _updateCoroutine = StartCoroutine(UpdateLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(_updateCoroutine);
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            if (IsPlaying)
            {
                if (
                    (_currentFrame >= _frames.Count - 1 && !ReversePlaying) ||
                    (_currentFrame <= 0 && ReversePlaying)
                )
                {
                    AnimationFinishedProcess();
                }

                _timeLeftSinceLastFrame += TimeScaleDepended ? Time.deltaTime : 1f / FPS;

                if (_timeLeftSinceLastFrame >= 1f / (float)FPS)
                {
                    if (ReversePlaying)
                    {
                        CurrentFrame--;
                    }
                    else
                    {
                        CurrentFrame++;
                    }
                    _timeLeftSinceLastFrame = 1f / (float)FPS - _timeLeftSinceLastFrame;
                }
            }

            if (TimeScaleDepended)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds(1 / FPS);
            }
        }
    }

    private void UpdateFrame()
    {
        _imageComponent.sprite = _frames[_currentFrame];
    }

    private void AnimationFinishedProcess()
    {
        AnimationFinished?.Invoke(this, EventArgs.Empty);

        switch (AnimationPlayMode)
        {
            case AnimatedSpritePlayMode.LOOP:
                break;
            case AnimatedSpritePlayMode.ONESHOT_HIDE_ON_FINISH:
                IsPlaying = false;
                _imageComponent.enabled = false;
                break;
            case AnimatedSpritePlayMode.ONESHOT_DESTROY_ON_FINISH:
                Destroy(gameObject);
                break;
            case AnimatedSpritePlayMode.ONESHOT_KEEP_ON_FINISH:
                IsPlaying = false;
                break;
        }
    }
}