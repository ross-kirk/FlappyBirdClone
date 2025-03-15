using System;
using Core;
using UnityEngine;

namespace Game
{
    public class GameStateController : MonoBehaviour
    {
        public event Action<int> OnScoreUpdate;
        public event Action OnGameOver;
        public event Action OnStartGame;
        public event Action OnPauseGame;
        public event Action OnRestartGame;
        
        public int CurrentScore { get; private set; }
        public int LastScore { get; private set; }
        
        private readonly StateMachine stateMachine = new StateMachine();
        private PipeController pipeController;
        private Player player;
        private GameOverPopup gameOverPopup;

        public GameState CurrentState => stateMachine.CurrentState switch
        {
            PlayState => GameState.Playing,
            PausedState => GameState.Paused,
            GameOverState => GameState.GameOver,
            NewGameState => GameState.NewGame,
            _ => GameState.NewGame 
        };

        public static GameStateController Instance { get; private set; }

        private void OnEnable()
        {
            Setup();
        }

        public void RestartGame()
        {
            stateMachine.ChangeState(new NewGameState(player, pipeController));
            OnRestartGame?.Invoke();
            StartGame();
        }

        public void PauseGame()
        {
            stateMachine.ChangeState(new PausedState(pipeController, player));
            OnPauseGame?.Invoke();
        }

        public void StartGame()
        {
            stateMachine.ChangeState(new PlayState(pipeController, player));
            OnStartGame?.Invoke();
        }

        public void GameOver()
        {
            stateMachine.ChangeState(new GameOverState(player, pipeController, gameOverPopup));
            OnGameOver?.Invoke();
        }

        public void IncrementScore()
        {
            CurrentScore += 1;
            OnScoreUpdate?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            LastScore = CurrentScore;
            CurrentScore = 0;
            OnScoreUpdate?.Invoke(CurrentScore);
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
                pipeController = FindFirstObjectByType<PipeController>();
            }

            if (player == null)
            {
                player = FindFirstObjectByType<Player>();
            }

            if (gameOverPopup == null)
            {
                gameOverPopup = FindFirstObjectByType<GameOverPopup>(FindObjectsInactive.Include);
            }
        }
    }
}