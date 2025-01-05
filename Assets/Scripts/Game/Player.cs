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
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
            FreezePlayer();
            playerInput = GetComponent<PlayerInput>() ?? gameObject.AddComponent<PlayerInput>();
            jumpAction = playerInput.actions["Jump"];
            jumpAction.performed += _ => HandleJump();
        }

        public void FreezePlayer()
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void UnFreezePlayer()
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        private void HandleJump()
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}