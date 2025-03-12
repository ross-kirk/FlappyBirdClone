using Game;

namespace Core
{
    public class GameOverState : IState
    {
        private Player _player;
        private PipeController _pipeController;
        
        public GameOverState(Player player, PipeController pipeController)
        {
            _player = player;
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