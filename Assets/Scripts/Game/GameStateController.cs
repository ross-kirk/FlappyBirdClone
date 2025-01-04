using System;
using Core;
using UnityEngine;

namespace Game
{
    public class GameStateController : MonoBehaviour
    {
        private readonly StateMachine stateMachine = new StateMachine();
        private PipeController pipeController;
        //TODO add player ref

        public static GameStateController Instance { get; private set; }

        public void PauseGame()
        {
            stateMachine.ChangeState(new PausedState(pipeController));
        }

        public void StartGame()
        {
            stateMachine.ChangeState(new PlayState(pipeController));
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
            //TODO add player setup
        }
    }
}