using System;
using TMPro;
using UnityEngine;

namespace Game
{
    public class GameOverPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text lastScoreText;

        private void OnEnable()
        {
            lastScoreText.text = $"Last Score: {GameStateController.Instance.LastScore}";
        }
    }
}