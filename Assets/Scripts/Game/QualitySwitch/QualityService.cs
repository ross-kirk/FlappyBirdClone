using System;
using UnityEngine;

namespace Game.QualitySwitch
{
    public class QualityService : MonoBehaviour
    {
        public static QualityService Instance { get; private set; }
        public static event Action<QualityType> QualityChanged;

        [SerializeField] private QualityType defaultQualityType = QualityType.ProgrammerArt;
        public static QualityType CurrentQuality { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CurrentQuality = defaultQualityType;
            DontDestroyOnLoad(gameObject);
        }

        public static void SetQuality(QualityType type)
        {
            if (CurrentQuality == type)
            {
                return;
            }

            CurrentQuality = type;
            QualityChanged?.Invoke(CurrentQuality);
        }

        private void OnEnable()
        {
            QualityChanged?.Invoke(CurrentQuality);
        }
    }
}