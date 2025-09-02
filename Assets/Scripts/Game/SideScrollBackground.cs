using System;
using Game.QualitySwitch;
using UnityEngine;
using Utils;

namespace Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SideScrollBackground : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 0.5f;
        [SerializeField] private bool reverseDirection = false;
        private float backgroundWidth;
        private SpriteRenderer _renderer;
        private SpriteRendererQualitySwitch _qualitySwitch;
        private bool canMove = true;

        private Vector3 startPosition;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _qualitySwitch = GetComponent<SpriteRendererQualitySwitch>();
            SetBackgroundWidth();
            BuildBounds();
        }

        private void Start()
        {
            startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            if (!canMove)
            {
                return;
            }
            
            var newPosition = Mathf.Repeat(Time.time * scrollSpeed, backgroundWidth);
            transform.position = !reverseDirection
                ? startPosition + Vector3.left * newPosition
                : startPosition + Vector3.right * newPosition;
        }

        private void SetBackgroundWidth()
        {
            if (_renderer != null)
            {
                backgroundWidth = _renderer.size.x * transform.localScale.x;
            }
        }

        private void BuildBounds()
        {
            if (backgroundWidth > 0)
            {
                var leftChild = new GameObject("LeftChild");
                leftChild.transform.SetParent(gameObject.transform);
                leftChild.transform.localPosition = new Vector3(-backgroundWidth, 0, 0);
                leftChild.transform.localScale = new Vector3(1, 1, 1);
                _renderer.CopyComponent(leftChild);
                leftChild.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
                _qualitySwitch.CopyComponent(leftChild);

                var rightChild = new GameObject("RightChild");
                rightChild.transform.SetParent(gameObject.transform);
                rightChild.transform.localPosition = new Vector3(backgroundWidth, 0, 0);
                rightChild.transform.localScale = new Vector3(1, 1, 1);
                _renderer.CopyComponent(rightChild);
                rightChild.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
                _qualitySwitch.CopyComponent(rightChild);
                
                if (TryGetComponent<Collider>(out var parentCollider))
                {
                    parentCollider.CopyComponent(leftChild);
                    parentCollider.CopyComponent(rightChild);
                }

                if (TryGetComponent<Rigidbody2D>(out var parentRb))
                {
                    var leftRb = leftChild.AddComponent<Rigidbody2D>();
                    var rightRb = rightChild.AddComponent<Rigidbody2D>();

                    leftRb.bodyType = parentRb.bodyType; 
                    leftRb.constraints = parentRb.constraints;

                    rightRb.bodyType = parentRb.bodyType; 
                    rightRb.constraints = parentRb.constraints;
                }
            }
        }

        public void StopMovement()
        {
            canMove = false;
        }

        public void StartMovement()
        {
            canMove = true;
        }
    }
}