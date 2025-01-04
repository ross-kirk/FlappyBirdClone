using Game;

namespace Core
{
    public class PlayState : IState
    {
        private PipeController _pipeController;
        //TODO add player ref

        public PlayState(PipeController pipeController)
        {
            _pipeController = pipeController;
            //todo add player DI
        }
        
        public void Enter()
        {
            _pipeController.StartPipes();
            _pipeController.StartSpawner();
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