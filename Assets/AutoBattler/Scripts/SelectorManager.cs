using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorManager : MonoBehaviour
{
    public static SelectorManager Instance { get; private set; }
  
    public List<PosBattlerDropZone> dragAndDropUnits = new List<PosBattlerDropZone>(3); // Lista de objetos PosBattlerDropZone asociados a las unidades seleccionadas
    public List<UnitData> selectedUnitsData = new List<UnitData>(3); // Lista de datos de las unidades seleccionadas
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void Select()
    {
        for(int i = 0; i < dragAndDropUnits.Count; i++)
        {
            if(dragAndDropUnits[i].currentUnit != null)
            {
                //DataManagerAutoBattler.playerUnits[i] = dragAndDropUnits[i].currentUnit.unitData; // Asigna los datos de la unidad seleccionada a la lista del DataManager
                DataManagerAutoBattler.playerUnits.Insert(i, dragAndDropUnits[i].currentUnit.unitData); // Inserta los datos de la unidad seleccionada en la posición correspondiente de la lista del DataManager
                selectedUnitsData[i] = dragAndDropUnits[i].currentUnit.unitData; // Asigna los datos de la unidad seleccionada a la lista de datos seleccionados
            }
            else
            {
                //DataManagerAutoBattler.playerUnits[i] = null; // Si no hay unidad seleccionada, asigna null
                DataManagerAutoBattler.playerUnits.Insert(i, null); // Inserta null en la posición correspondiente de la lista del DataManager
                selectedUnitsData[i] = null; // Si no hay unidad seleccionada, asigna null a la lista de datos seleccionados
            }
        }
        SceneManager.LoadScene(1);
    }
    
}
