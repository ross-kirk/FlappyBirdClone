using UnityEngine;

namespace Game
{
    public class Pipe : MonoBehaviour
    {
        public float Speed { get; private set; }

        public void SetSpeed(float speed)
        {
            Speed = speed;
        }

        private void FixedUpdate()
        {
            if (Speed > 0f)
            {
                transform.Translate(Vector2.left * (Speed * Time.deltaTime));
            }
        }
    }
}