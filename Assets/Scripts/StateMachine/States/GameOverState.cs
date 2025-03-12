using Game;

namespace Core
{
    public class GameOverState : IState
    {
        private readonly Player _player;
        private readonly PipeController _pipeController;
        
        public GameOverState(Player player, PipeController pipeController)
        {
            _player = player;
            _pipeController = pipeController;
        }

        public void Enter()
        {
            _pipeController.StopSpawner();
            _pipeController.StopPipes();
            _player.FreezePlayer();
        }

        public void Exit()
        {
        }

        public void Execute()
        {
        }
    }
}