using Core;
using UnityEngine;

namespace Game
{
    public class PipeController : MonoBehaviour
    {
        [SerializeField] private float pipeSpeed;
        [SerializeField] private float pipeSpawnDelay;
        
        private Pipe[] pipes;

        private StateMachine gameStateMachine = new StateMachine();

        public void StopPipes()
        {
            foreach (var pipe in pipes)
            {
                pipe.SetSpeed(0);
            }
        }
    }
}