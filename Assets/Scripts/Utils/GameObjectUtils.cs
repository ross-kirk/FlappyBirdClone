using UnityEngine;

namespace Utils
{
    public static class GameObjectUtils
    {
        /// <summary>
        /// Set state of game object safely only if not already set to intended value
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="value"></param>
        public static void SetActiveSafe(this GameObject gameObject, bool value)
        {
            if (gameObject == null)
            {
                Debug.LogWarning($"Game object was null, could not set {value}");
                return;
            }
            
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
            }

        }
        
        /// <summary>
        /// Null safe way to toggle active state of game object
        /// </summary>
        /// <param name="gameObject"></param>
        public static void ToggleActiveSafe(this GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
            else
            {
                Debug.LogError("Game object was null, could not toggle active state");
            }
        }

        public static T AddComponent<T>(this GameObject gameObject, T original) where T : Component
        {
            return original.CopyComponent(gameObject);
        }
    }
}