using Game;

namespace Core
{
    public class PausedState : IState
    {
        private readonly PipeController _pipeController;

        public PausedState(PipeController pipeController)
        {
            _pipeController = pipeController;
        }
        public void Enter()
        {
            _pipeController.StopSpawner();
            _pipeController.StopPipes();
        }

        public void Exit()
        {
        }

        public void Execute()
        {
        }
    }
}