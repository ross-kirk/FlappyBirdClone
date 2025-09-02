using Game;

namespace Core
{
    public class PausedState : IState
    {
        private readonly PipeController _pipeController;
        private readonly IPlayer _player;
        
        public PausedState(PipeController pipeController, IPlayer player)
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
        }

        public void Execute()
        {
        }
    }
}