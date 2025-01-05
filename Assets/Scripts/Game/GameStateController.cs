using System;
using Core;
using UnityEngine;

namespace Game
{
    public class GameStateController : MonoBehaviour
    {
        private readonly StateMachine stateMachine = new StateMachine();
        private PipeController pipeController;
        private Player player; 

        public static GameStateController Instance { get; private set; }

        public void PauseGame()
        {
            stateMachine.ChangeState(new PausedState(pipeController, player));
        }

        public void StartGame()
        {
            Setup();
            stateMachine.ChangeState(new PlayState(pipeController, player));
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            Setup();
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            PauseGame();
        }

        private void Setup()
        {
            if (pipeController == null)
            {
                pipeController = FindFirstObjectByType<PipeController>() ?? 
                                 new GameObject().AddComponent<PipeController>();
            }

            if (player == null)
            {
                player = FindFirstObjectByType<Player>() ??
                         new GameObject().AddComponent<Player>();
            }
        }
    }
}