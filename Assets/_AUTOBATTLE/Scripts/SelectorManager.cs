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
        Debug.Log($"SelectorManager: Select() iniciado. DropZones registradas: {dragAndDropUnits.Count}");

        if (DataManagerAutoBattler.playerUnits == null)
        {
            DataManagerAutoBattler.playerUnits = new List<UnitData>(4);
            Debug.LogWarning("SelectorManager: DataManagerAutoBattler.playerUnits estaba null. Se creo una lista nueva.");
        }

        DataManagerAutoBattler.playerUnits.Clear();
        selectedUnitsData.Clear();

        for (int i = 0; i < dragAndDropUnits.Count; i++)
        {
            PosBattlerDropZone dropZone = dragAndDropUnits[i];

            if (dropZone == null)
            {
                Debug.LogWarning($"SelectorManager: Slot {i} no tiene PosBattlerDropZone asignado en el inspector. Se guardara null.");
                DataManagerAutoBattler.playerUnits.Add(null);
                selectedUnitsData.Add(null);
                continue;
            }

            if (dropZone.currentUnit != null)
            {
                UnitData unitData = dropZone.currentUnit.unitData;
                DataManagerAutoBattler.playerUnits.Add(unitData);
                selectedUnitsData.Add(unitData);

                if (unitData == null)
                {
                    Debug.LogWarning($"SelectorManager: Slot {i} esta ocupado por '{dropZone.currentUnit.name}', pero su DragAndDrop.unitData esta NULL. BattleManager no podra instanciar esta unidad.");
                }
                else
                {
                    string prefabName = unitData.unitPrefab != null ? unitData.unitPrefab.name : "NULL";
                    Debug.Log($"SelectorManager: Slot {i} agrego UnitData '{unitData.unitName}' ({unitData.name}). unitPrefab: {prefabName}");
                }
            }
            else
            {
                DataManagerAutoBattler.playerUnits.Add(null);
                selectedUnitsData.Add(null);
                Debug.Log($"SelectorManager: Slot {i} vacio. Se guardo null.");
            }
        }

        Debug.Log($"SelectorManager: Se guardaron {DataManagerAutoBattler.playerUnits.Count} entradas en DataManagerAutoBattler.playerUnits. Cargando escena 1.");
        SceneManager.LoadScene(1);
    }
    
}
