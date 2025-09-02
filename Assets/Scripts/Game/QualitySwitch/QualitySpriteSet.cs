using System;
using UnityEngine;

namespace Game.QualitySwitch
{
    [Serializable]
    public class QualitySpriteSet
    {
        public Sprite ProgrammerArt;
        public Color ProgrammerArtColorTint;
        public Sprite SlightlyProgrammerArt;
        public Sprite RealArt;
        public Color NormalColorTint;

        public Sprite GetSprite(QualityType qualityType)
        {
            switch (qualityType)
            {
                case QualityType.ProgrammerArt:
                    return ProgrammerArt;
                case QualityType.SlightlyBetterProgrammerArt:
                    return SlightlyProgrammerArt;
                default:
                case QualityType.RealArt:
                    return RealArt;
            }
        }
    }
}