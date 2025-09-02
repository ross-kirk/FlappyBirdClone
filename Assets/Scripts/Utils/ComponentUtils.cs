using System.Reflection;
using UnityEngine;

namespace Utils
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Copy component from a gameobject
        /// </summary>
        /// <param name="original"></param>
        /// <param name="dest"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns> The new component onto the destination component </returns>
        public static T CopyComponent<T>(this T original, GameObject dest) where T : Component
        {
            var copy = dest.AddComponent<T>();

            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite) continue;
                
                property.SetValue(copy, property.GetValue(original, null), null);
            }

            return copy;
        }
    }
}