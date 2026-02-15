using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ComboEncounter : MonoBehaviour
{
    const float CHANGE_COMBO_BG_SPEED = 25f;
    const float SCORE_ENCOUNT_DELAY_SECONDS = 1f;
    const float SCORE_ENCOUNT_SPEED_PER_SECOND = 100f;

    private int _currentDisplayedScore = 0;
    private int _currentAddingScore = 0;
    private Coroutine _setBgCoroutine = null;
    private Coroutine _scoreEncountCoroutine = null;

    [SerializeField] private RectTransform _visualContainer;
    [SerializeField] private Image _comboBg;
    [SerializeField] private Image _oldComboBg;
    [SerializeField] private Image _comboLastTimeFillImage;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private ShakableObject _comboInfoShaking;

    public void UpdateCurrentCombo()
    {
        if (_comboText.IsDestroyed()) return;
        _comboText.text = ScoreManager.Instance.CurrentCombo.ToString();

        if (_oldComboBg.IsDestroyed() || _comboBg.IsDestroyed()) return;
        foreach (ScoreManager.ComboState state in ScoreManager.Instance.ComboStates)
        {
            if (state.MinCombo <= ScoreManager.Instance.CurrentCombo && state.MaxCombo > ScoreManager.Instance.CurrentCombo)
            {
                if (_comboBg.sprite != state.BgSprite)
                {
                    if (_setBgCoroutine != null) StopCoroutine(_setBgCoroutine);
                    _setBgCoroutine = StartCoroutine(ShowNewBg(state.BgSprite));
                    _comboInfoShaking.ContantShakingForce = state.Shaking;
                }

                return;
            }
        }
    }

    private IEnumerator ShowNewBg(Sprite sprite)
    {
        if (!_oldComboBg.IsDestroyed() && !_comboBg.IsDestroyed())
        {
            _oldComboBg.sprite = _comboBg.sprite;
            _oldComboBg.transform.localScale = Vector3.one;
            _comboBg.sprite = sprite;
            _comboBg.transform.localScale = Vector3.zero;
            _comboBg.enabled = sprite != null;
        }


        float progress = 0f;

        while (progress < 1f - 0.005f)
        {
            progress = math.lerp(progress, 1f, Time.deltaTime * CHANGE_COMBO_BG_SPEED);

            _oldComboBg.transform.localScale = Vector3.one * (1f - progress);
            _comboBg.transform.localScale = Vector3.one * progress;

            yield return new WaitForEndOfFrame();
        }
        _oldComboBg.transform.localScale = Vector3.zero;
        _comboBg.transform.localScale = Vector3.one;

        _setBgCoroutine = null;
    }

    public void UpdateCurrentMultiplier()
    {
        if (_multiplierText.IsDestroyed()) return;
        _multiplierText.text = "x" + ScoreManager.Instance.CurrentMultiplier.ToString();
    }

    public void AddScore(int score)
    {
        _currentAddingScore += score;

        if (_scoreEncountCoroutine != null) StopCoroutine(_scoreEncountCoroutine);
        _scoreEncountCoroutine = StartCoroutine(ScoreEncount());
    }

    public void ForceSetScore(int score)
    {
        _currentDisplayedScore = ScoreManager.Instance.CurrentScore;
        _currentAddingScore = 0;

        if (_scoreText.IsDestroyed()) return;
        _scoreText.text = score.ToString("00000");
    }

    private IEnumerator ScoreEncount()
    {
        if (!_scoreText.IsDestroyed())
        {
            _scoreText.text = _currentAddingScore.ToString() + "+" + _currentDisplayedScore.ToString("00000");
        }

        yield return new WaitForSeconds(SCORE_ENCOUNT_DELAY_SECONDS);

        while (_currentAddingScore > 0)
        {
            int scoreChange = (int)math.ceil(Time.deltaTime * SCORE_ENCOUNT_SPEED_PER_SECOND);
            if (_currentAddingScore < scoreChange) scoreChange = _currentAddingScore;

            _currentAddingScore -= scoreChange;
            _currentDisplayedScore += scoreChange;

            if (!_scoreText.IsDestroyed())
            {
                _scoreText.text = _currentAddingScore.ToString() + "+" + _currentDisplayedScore.ToString("00000");
            }

            yield return new WaitForEndOfFrame();
        }

        if (!_scoreText.IsDestroyed())
        {
            _scoreText.text = _currentDisplayedScore.ToString("00000");
        }
    }

    public void UpdateComboLastTime()
    {
        if (_comboLastTimeFillImage.IsDestroyed()) return;
        _comboLastTimeFillImage.fillAmount = ScoreManager.Instance.ComboLastTime / ScoreManager.Instance.ResetComboDelay;
    }

    private void Start()
    {
        UpdateCurrentCombo();
        UpdateCurrentMultiplier();
        UpdateComboLastTime();

        _currentDisplayedScore = ScoreManager.Instance.CurrentScore;
        _scoreText.text = _currentDisplayedScore.ToString("00000");
    }
}