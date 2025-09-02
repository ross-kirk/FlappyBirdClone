using System;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerUpdate : MonoBehaviour, IPlayer
    {
        [SerializeField] private float verticalSpeed = 4f;
        [SerializeField] private float rotationSpeed = 4f;
        [SerializeField] private float maxTilt = 15f;
        [SerializeField] private float gameOverGravity = 2.5f;
        [SerializeField] private float fallRotationLerp = 2f;
        [SerializeField] private float fallDownAngle = -125f;

        private Rigidbody2D rb;
        private PlayerInput playerInput;
        private Vector2 initialLocation;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            playerInput = GetComponent<PlayerInput>();
            FreezePlayer();
        }

        private void OnEnable()
        {
            if (GameStateController.Instance != null)
                GameStateController.Instance.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            if (GameStateController.Instance != null)
                GameStateController.Instance.OnGameOver -= HandleGameOver;
        }

        private void Start()
        {
            initialLocation = transform.position;
        }

        public void RestartPosition()
        {
            transform.rotation = Quaternion.identity;
            transform.position = initialLocation;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        public void FreezePlayer()
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        public void UnfreezePlayer()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private void HandleGameOver()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = gameOverGravity;
            rb.freezeRotation = true;
        }

        private void Update()
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic) return;

            if (GameStateController.Instance.CurrentState == GameState.Playing)
            {
                float verticalInput = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
                if (Mouse.current.leftButton.isPressed)
                    verticalInput = Mathf.Clamp((Mouse.current.position.ReadValue().y - Screen.height / 2f) / (Screen.height / 2f), -1f, 1f);
#elif UNITY_ANDROID || UNITY_IOS
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                    verticalInput = Mathf.Clamp((Touchscreen.current.primaryTouch.position.ReadValue().y - Screen.height / 2f) / (Screen.height / 2f), -1f, 1f);
#endif
                rb.linearVelocity = new Vector2(0f, verticalInput * verticalSpeed);

                var targetRotation = Quaternion.Euler(0f, 0f, verticalInput * maxTilt);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                return;
            }

            if (GameStateController.Instance.CurrentState == GameState.GameOver)
            {
                var targetRotation = Quaternion.Euler(0f, 0f, fallDownAngle);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * fallRotationLerp);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("KillLayer") &&
                GameStateController.Instance.CurrentState == GameState.Playing)
            {
                GameStateController.Instance.GameOver();
            }

            if (other.CompareTag("Goal") &&
                GameStateController.Instance.CurrentState == GameState.Playing)
            {
                GameStateController.Instance.IncrementScore();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("KillLayer") &&
                GameStateController.Instance.CurrentState == GameState.Playing)
            {
                GameStateController.Instance.GameOver();
            }

            if (other.gameObject.CompareTag("KillLayer") &&
                GameStateController.Instance.CurrentState == GameState.GameOver)
            {
                FreezePlayer();
            }
        }
    }
}
