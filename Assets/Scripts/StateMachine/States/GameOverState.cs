using Game;

namespace Core
{
    public class GameOverState : IState
    {
        private readonly IPlayer _player;
        private readonly PipeController _pipeController;
        private readonly GameOverPopup _gameOverPopup;
        private readonly AudioEvent _gameOverAudioEvent;
        
        public GameOverState(
            IPlayer player, 
            PipeController pipeController, 
            GameOverPopup gameOverPopup, 
            AudioEvent gameOverAudioEvent)
        {
            _player = player;
            _pipeController = pipeController;
            _gameOverPopup = gameOverPopup;
            _gameOverAudioEvent = gameOverAudioEvent;
        }

        public void Enter()
        {
            _pipeController.StopSpawner();
            _pipeController.StopPipes();
            GameStateController.Instance.ResetScore();
            _gameOverPopup.gameObject.SetActive(true);
            AudioComponent.Instance.Player.PlaySound(_gameOverAudioEvent);
        }

        public void Exit()
        {
            _gameOverPopup.gameObject.SetActive(false);
        }

        public void Execute()
        {
        }
    }
}