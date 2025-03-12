using System;
using TMPro;
using UnityEngine;

namespace Game
{
    public class ScorePresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private void Start()
        {
            GameStateController.Instance.OnScoreUpdate += UpdateScoreValue;
        }

        private void OnDisable()
        {
            GameStateController.Instance.OnScoreUpdate -= UpdateScoreValue;
        }

        private void UpdateScoreValue(int value)
        {
            scoreText.text = $"Score: {value}";
        }
    }
}