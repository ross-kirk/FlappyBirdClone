using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 4f;
        [SerializeField] private float rotationSpeed = 0.8f;
        [SerializeField] private float downwardAngle = -125f;
        [SerializeField] private float jumpRotationAngle = 30f;
        private Rigidbody2D rb;
        private PlayerInput playerInput;
        private InputAction jumpAction;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
            playerInput = GetComponent<PlayerInput>();
            jumpAction = playerInput.actions["Jump"];
            jumpAction.performed += _ => HandleJump();
            FreezePlayer();
        }

        public void FreezePlayer()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        public void UnfreezePlayer()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        private void HandleJump()
        {
            if (rb.bodyType == RigidbodyType2D.Kinematic)
                return;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            transform.rotation = Quaternion.Euler(0, 0, -jumpRotationAngle);
        }

        private void Update()
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
                return;

            var targetRotation = Quaternion.Euler(0, 0, downwardAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}