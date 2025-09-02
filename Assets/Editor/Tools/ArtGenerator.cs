using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Editor
{
    public class ArtGenerator : EditorWindow
    {
        private int width = 512;
        private int height = 512;

        [MenuItem("Tools/Generate White PNG")]
        public static void ShowWindow()
        {
            GetWindow<ArtGenerator>("Art Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("White PNG Settings", EditorStyles.boldLabel);
            width = EditorGUILayout.IntField("Width", width);
            height = EditorGUILayout.IntField("Height", height);

            if (GUILayout.Button("Genereate PNG"))
            {
                GenerateWhitePNG(width, height);
            }
        }

        private void GenerateWhitePNG(int w, int h)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            var path = EditorUtility.SaveFilePanel("Save PNG", Application.dataPath, "WhiteImage", "png");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.Refresh();
                Debug.Log($"White PNG saved to: {path}");
            }
        }
    }
}

