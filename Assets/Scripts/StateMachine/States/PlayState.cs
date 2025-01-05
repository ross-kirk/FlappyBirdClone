using Game;

namespace Core
{
    public class PlayState : IState
    {
        private readonly PipeController _pipeController;
        private readonly Player _player;

        public PlayState(PipeController pipeController, Player player)
        {
            _pipeController = pipeController;
            _player = player;
        }
        
        public void Enter()
        {
            _pipeController.StartPipes();
            _pipeController.StartSpawner();
            _player.UnFreezePlayer();
        }

        public void Exit()
        {
            _pipeController.StopSpawner();
        }

        public void Execute()
        {
        }
    }
}