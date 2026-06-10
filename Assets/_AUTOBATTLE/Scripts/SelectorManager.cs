using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorManager : MonoBehaviour
{
    public static SelectorManager Instance { get; private set; }
  
    public List<PosBattlerDropZone> dragAndDropUnits = new List<PosBattlerDropZone>(4); // Lista de objetos PosBattlerDropZone asociados a las unidades seleccionadas
    public List<UnitData> selectedUnitsData = new List<UnitData>(4); // Lista de datos de las unidades seleccionadas
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void Select()
    {
        if (DataManagerAutoBattler.playerUnits == null)
        {
            DataManagerAutoBattler.playerUnits = new List<UnitData>(4);
        }

        DataManagerAutoBattler.playerUnits.Clear();
        selectedUnitsData.Clear();

        for (int i = 0; i < dragAndDropUnits.Count; i++)
        {
            if (dragAndDropUnits[i].currentUnit != null)
            {
                DataManagerAutoBattler.playerUnits.Add(dragAndDropUnits[i].currentUnit.unitData);
                selectedUnitsData.Add(dragAndDropUnits[i].currentUnit.unitData);
            }
            else
            {
                DataManagerAutoBattler.playerUnits.Add(null);
                selectedUnitsData.Add(null);
            }
        }

        SceneManager.LoadScene(2);
    }
    
}
