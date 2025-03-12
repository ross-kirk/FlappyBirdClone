using Game;

namespace Core
{
    public class NewGameState : IState
    {
        private readonly Player _player;
        private readonly PipeController _pipeController;

        public NewGameState(Player player, PipeController pipeController)
        {
            _player = player;
            _pipeController = pipeController;
        }
        
        public void Enter()
        {
            _player.RestartPosition();
            GameStateController.Instance.ResetScore();
        }

        public void Exit()
        {
            GameStateController.Instance.StartGame();
        }

        public void Execute()
        {
        }
    }
}