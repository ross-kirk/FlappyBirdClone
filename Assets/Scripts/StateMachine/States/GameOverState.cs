using Game;

namespace Core
{
    public class GameOverState : IState
    {
        private readonly Player _player;
        private readonly PipeController _pipeController;
        private readonly GameOverPopup _gameOverPopup;
        
        public GameOverState(Player player, PipeController pipeController, GameOverPopup gameOverPopup)
        {
            _player = player;
            _pipeController = pipeController;
            _gameOverPopup = gameOverPopup;
        }

        public void Enter()
        {
            _pipeController.StopSpawner();
            _pipeController.StopPipes();
            GameStateController.Instance.ResetScore();
            _gameOverPopup.gameObject.SetActive(true);
        }

        public void Exit()
        {
            _gameOverPopup.gameObject.SetActive(false);
            _pipeController.RemoveAllPipes();
        }

        public void Execute()
        {
        }
    }
}