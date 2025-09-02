using UnityEngine;
using UnityEngine.UI;

namespace Game.QualitySwitch
{
    [RequireComponent(typeof(Image))]
    public class UIImageQualitySwitch : MonoBehaviour, IQualitySwitch
    {
        [SerializeField] private Image target;
        [SerializeField] private QualitySpriteSet sprites;
        [SerializeField] private bool preserveAspect = true;

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

            target.preserveAspect = preserveAspect;
            target.SetNativeSize();
        }

        private void ResetTarget()
        {
            if (!target || !target.gameObject != gameObject)
            {
                target = GetComponent<Image>();
            }
        }
    }
}