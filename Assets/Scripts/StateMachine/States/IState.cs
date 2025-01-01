namespace Core
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Execute();
    }
}