using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class Player  : MonoBehaviour
    {
        [SerializeField] private float jumpForce = 4f;
        private Rigidbody2D rb;
        private PlayerInput playerInput;
        private InputAction jumpAction;
        private RigidbodyConstraints2D? cachedConstraints;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
            FreezePlayer();
            playerInput = GetComponent<PlayerInput>();
            jumpAction = playerInput.actions["Jump"];
            jumpAction.performed += _ => HandleJump();
        }

        public void FreezePlayer()
        {
            cachedConstraints = rb.constraints;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void UnFreezePlayer()
        {
            if (cachedConstraints.HasValue && cachedConstraints.Value != RigidbodyConstraints2D.FreezeAll)
            {
                rb.constraints = cachedConstraints ?? RigidbodyConstraints2D.None;
            }
        }

        private void HandleJump()
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}