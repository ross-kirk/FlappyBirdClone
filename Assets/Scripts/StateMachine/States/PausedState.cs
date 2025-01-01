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
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}