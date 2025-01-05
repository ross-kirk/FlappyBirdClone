using Game;

namespace Core
{
    public class PausedState : IState
    {
        private readonly PipeController _pipeController;
        private readonly Player _player;
        
        public PausedState(PipeController pipeController, Player player)
        {
            _pipeController = pipeController;
            _player = player;
        }
        public void Enter()
        {
            _pipeController.StopSpawner();
            _pipeController.StopPipes();
            _player.FreezePlayer();
        }

        public void Exit()
        {
            _player.UnFreezePlayer();
        }

        public void Execute()
        {
        }
    }
}