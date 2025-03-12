using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class BackgroundController : MonoBehaviour
    {
        private List<SideScrollBackground> _sideScrollBackgrounds;

        private void Start()
        {
            _sideScrollBackgrounds = GetComponentsInChildren<SideScrollBackground>().ToList();
            GameStateController.Instance.OnPauseGame += StopBackground;
            GameStateController.Instance.OnGameOver += StopBackground;
            GameStateController.Instance.OnStartGame += StartBackground;
        }

        private void OnDisable()
        {
            GameStateController.Instance.OnPauseGame -= StopBackground;
            GameStateController.Instance.OnGameOver -= StopBackground;
            GameStateController.Instance.OnStartGame -= StartBackground;
        }

        private void StopBackground()
        {
            foreach (var background in _sideScrollBackgrounds)
            {
                background.StopMovement();
            }
        }

        private void StartBackground()
        {
            foreach (var background in _sideScrollBackgrounds)
            {
                background.StartMovement();
            }
        }
    }
}