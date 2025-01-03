using System;
using UnityEngine;

namespace Game
{
    public class Pipe : MonoBehaviour
    {
        public event Action<Pipe> OnPipeDestroyed;
        public float Speed { get; private set; }

        public void SetSpeed(float speed)
        {
            Speed = speed;
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
            OnPipeDestroyed?.Invoke(this);
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