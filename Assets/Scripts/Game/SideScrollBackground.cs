using System;
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

        private Vector3 startPosition;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            SetBackgroundWidth();
            BuildBounds();
        }

        void Start()
        {
            startPosition = transform.position;
        }

        void FixedUpdate()
        {
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

                var rightChild = new GameObject("RightChild");
                rightChild.transform.SetParent(gameObject.transform);
                rightChild.transform.localPosition = new Vector3(backgroundWidth, 0, 0);
                rightChild.transform.localScale = new Vector3(1, 1, 1);
                _renderer.CopyComponent(rightChild);
            }
        }
    }
}