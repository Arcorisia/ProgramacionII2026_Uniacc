using UnityEngine;
using UnityEngine.UI;

public class ShowButtonBySelectedUnits : MonoBehaviour
{
    [SerializeField] private SelectorManager selectorManager;
    [SerializeField] private Button buttonToShow;
    [SerializeField] private int minUnitsToShow = 3;
    [SerializeField] private int maxUnitsToShow = 4;

    private void Awake()
    {
        if (selectorManager == null)
        {
            selectorManager = SelectorManager.Instance;
        }

        UpdateButtonVisibility();
    }

    private void Update()
    {
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        if (selectorManager == null || buttonToShow == null)
        {
            return;
        }

        int selectedUnitsCount = CountSelectedUnits();
        bool shouldShowButton = selectedUnitsCount >= minUnitsToShow && selectedUnitsCount <= maxUnitsToShow;

        if (buttonToShow.gameObject.activeSelf != shouldShowButton)
        {
            buttonToShow.gameObject.SetActive(shouldShowButton);
        }
    }

    private int CountSelectedUnits()
    {
        int count = 0;

        for (int i = 0; i < selectorManager.dragAndDropUnits.Count; i++)
        {
            PosBattlerDropZone dropZone = selectorManager.dragAndDropUnits[i];

            if (dropZone != null && dropZone.currentUnit != null && dropZone.currentUnit.unitData != null)
            {
                count++;
            }
        }

        return count;
    }
}
