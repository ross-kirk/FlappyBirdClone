using System;
using TMPro;
using UnityEngine;

namespace Game.QualitySwitch
{
    public class QualityChangeControl : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;

        private void Awake()
        {
            if (!dropdown)
            {
                dropdown = GetComponent<TMP_Dropdown>();
            }
            
            dropdown.onValueChanged.AddListener(OnQualityDropdownChange);
            dropdown.value = (int) QualityService.CurrentQuality;
        }

        private void OnDestroy()
        {
            if (dropdown)
            {
                dropdown.onValueChanged.RemoveListener(OnQualityDropdownChange);
            }
        }

        private void OnQualityDropdownChange(int dropdownValue)
        {
            if (dropdownValue < 0 || dropdownValue > Enum.GetValues(typeof(QualityType)).Length)
            {
                return;
            }
            QualityService.SetQuality((QualityType) dropdownValue);
        }
    }
}