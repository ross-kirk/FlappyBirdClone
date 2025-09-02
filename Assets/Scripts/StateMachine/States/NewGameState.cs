using Game;

namespace Core
{
    public class NewGameState : IState
    {
        private readonly IPlayer _player;
        private readonly PipeController _pipeController;

        public NewGameState(IPlayer player, PipeController pipeController)
        {
            _player = player;
            _pipeController = pipeController;
        }
        
        public void Enter()
        {
            _player.RestartPosition();
            GameStateController.Instance.ResetScore();
            _pipeController.RemoveAllPipes();
        }

        public void Exit()
        {
        }

        public void Execute()
        {
        }
    }
}