using System;
using UnityEngine;

namespace Game.QualitySwitch
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererQualitySwitch : MonoBehaviour, IQualitySwitch
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private QualitySpriteSet sprites;

        private void Reset()
        {
            ResetTarget();
        }

        private void OnEnable()
        {
            ResetTarget();
            QualityService.QualityChanged += OnQualitySwitched;
            OnQualitySwitched(QualityService.CurrentQuality);
        }

        private void OnDisable()
        {
            QualityService.QualityChanged -= OnQualitySwitched;
        }

        public void OnQualitySwitched(QualityType type)
        {
            ResetTarget();

            target.sprite = sprites.GetSprite(type);
            switch (type)
            {
                case QualityType.ProgrammerArt:
                    target.color = sprites.ProgrammerArtColorTint;
                    break;
                case QualityType.SlightlyBetterProgrammerArt:
                case QualityType.RealArt:
                default:
                    target.color = sprites.NormalColorTint;
                    break;
            }
        }

        private void ResetTarget()
        {
            if (!target || !target.gameObject != gameObject)
            {
                target = GetComponent<SpriteRenderer>();
            }
        }
    }
}