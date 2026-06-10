using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum UnitType
{
    AtacaUnidadMasFuerte,
    AtacaUnidadMasDebil,
    AtacaUnidadAleatoria,
    SanaUnidadMasDebil,
    SanaUnidadAleatoria   

}


public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    //public List<UnitData> playerUnitDataList = new List<UnitData>();
    //public List<UnitData> enemyUnitDataList = new List<UnitData>();
    public List<Transform> playerUnitsParent = new List<Transform>();
    public List<Transform> enemyUnitsParent = new List<Transform>();
    public GameObject unitPrefab;

    public List<Unit> playerUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();
    public List<Unit> turnOrder = new List<Unit>();

    public GameObject damageTextPrefab;

    [Header("Battle End")]
    [SerializeField] private Button returnToPreviousSceneButton;
    [SerializeField] private float returnButtonDelay = 2f;

    private bool battleStarted;
    private bool battleEnded;
    private Coroutine showReturnButtonCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        ConfigureReturnButton();
    }

    private void ConfigureReturnButton()
    {
        if (returnToPreviousSceneButton == null)
        {
            return;
        }

        returnToPreviousSceneButton.gameObject.SetActive(false);
        returnToPreviousSceneButton.onClick.RemoveListener(LoadPreviousScene);
        returnToPreviousSceneButton.onClick.AddListener(LoadPreviousScene);
    }
    private void SortTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(playerUnits);
        turnOrder.AddRange(enemyUnits);
        turnOrder.Sort((a, b) => b.speed.CompareTo(a.speed));
    }

    private bool ValidateUnitPrefab()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("BattleManager: unitPrefab is not assigned. Assign the unit prefab in the inspector.");
            return false;
        }

        Unit rootUnit = unitPrefab.GetComponent<Unit>();
        if (rootUnit != null)
        {
            return true;
        }

        Unit childUnit = unitPrefab.GetComponentInChildren<Unit>(true);
        if (childUnit != null)
        {
            Debug.LogWarning($"BattleManager: unitPrefab '{unitPrefab.name}' no tiene Unit en la raiz, pero si en un hijo '{childUnit.name}'. Se usara ese componente. Recomendado: dejar Unit en la raiz del prefab contenedor.");
            return true;
        }

        Debug.LogError($"BattleManager: unitPrefab '{unitPrefab.name}' no contiene ningun componente Unit ni en la raiz ni en hijos. Asigna un prefab contenedor de combate que tenga Unit.cs.");
        return false;
    }

    private Unit GetUnitComponentFromInstance(GameObject unitObj, string context)
    {
        if (unitObj == null)
        {
            Debug.LogError($"BattleManager: {context} no tiene GameObject instanciado.");
            return null;
        }

        Unit unit = unitObj.GetComponent<Unit>();
        if (unit != null)
        {
            return unit;
        }

        unit = unitObj.GetComponentInChildren<Unit>(true);
        if (unit != null)
        {
            Debug.LogWarning($"BattleManager: {context} encontro Unit en un hijo '{unit.name}' del prefab instanciado '{unitObj.name}'. Funciona, pero es mas estable poner Unit en la raiz del prefab contenedor.");
            return unit;
        }

        Debug.LogError($"BattleManager: {context} el prefab instanciado '{unitObj.name}' no contiene componente Unit ni en la raiz ni en hijos.");
        return null;
    }
    
    public void GeneratePlayerUnits()
    {
        Debug.Log("BattleManager: GeneratePlayerUnits() iniciado.");
        playerUnits.Clear();

        if (playerUnitsParent == null || playerUnitsParent.Count == 0)
        {
            Debug.LogError("BattleManager: playerUnitsParent is not configured. Assign parent transforms in the inspector.");
            return;
        }

        if (DataManagerAutoBattler.playerUnits == null)
        {
            DataManagerAutoBattler.playerUnits = new List<UnitData>(4);
            Debug.LogWarning("BattleManager: DataManagerAutoBattler.playerUnits was null and was recreated.");
        }

        Debug.Log($"BattleManager: playerUnitsParent asignados: {playerUnitsParent.Count}. UnitData recibidos: {DataManagerAutoBattler.playerUnits.Count}.");

        int count = Mathf.Min(DataManagerAutoBattler.playerUnits.Count, playerUnitsParent.Count);
        if (count == 0)
        {
            Debug.LogWarning("BattleManager: No player units to generate or no player unit parent slots assigned.");
            return;
        }

        if (!ValidateUnitPrefab())
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            UnitData data = DataManagerAutoBattler.playerUnits[i];

            if (data == null)
            {
                Debug.LogWarning($"BattleManager: Aliado slot {i} trae UnitData NULL. Se saltara este slot.");
                continue;
            }

            Transform parent = playerUnitsParent[i];
            string unitModelPrefabName = data.unitPrefab != null ? data.unitPrefab.name : "NULL";
            string parentName = parent != null ? parent.name : "NULL";
            Debug.Log($"BattleManager: Preparando aliado slot {i}. UnitData '{data.unitName}' ({data.name}), unitPrefab: {unitModelPrefabName}, parent: {parentName}.");

            if (data.unitPrefab == null)
            {
                Debug.LogWarning($"BattleManager: Aliado slot {i} tiene UnitData '{data.name}', pero UnitData.unitPrefab esta NULL. Se instanciara el contenedor Unit, pero no tendra modelo interno.");
            }

            if (parent == null)
            {
                Debug.LogError($"BattleManager: playerUnitsParent[{i}] is null. Assign all parent transforms in the inspector.");
                continue;
            }

            GameObject unitObj = Instantiate(unitPrefab, parent);
            if (unitObj == null)
            {
                Debug.LogError($"BattleManager: Fallo Instantiate del contenedor unitPrefab para aliado slot {i}.");
                continue;
            }

            Debug.Log($"BattleManager: Contenedor aliado instanciado en slot {i}: '{unitObj.name}'.");

            Unit unit = GetUnitComponentFromInstance(unitObj, $"Aliado slot {i}:");
            if (unit == null)
            {
                Destroy(unitObj);
                continue;
            }

            unit.Initialize(data);
            playerUnits.Add(unit);
            Debug.Log($"BattleManager: Aliado slot {i} inicializado correctamente. Aliados activos: {playerUnits.Count}.");
        }

        if (DataManagerAutoBattler.playerUnits.Count > playerUnitsParent.Count)
        {
            Debug.LogWarning($"BattleManager: playerUnits list has {DataManagerAutoBattler.playerUnits.Count} entries but only {playerUnitsParent.Count} parent slots are assigned. Some units were skipped.");
        }

        Debug.Log($"BattleManager: GeneratePlayerUnits() terminado. Total aliados instanciados: {playerUnits.Count}.");
    }
    public void StartBattle(CombatEvent combatEvent)
    {
        battleStarted = false;
        battleEnded = false;
        HideReturnButton();

        string combatEventName = combatEvent != null ? combatEvent.name : "NULL";
        int enemyDataCount = combatEvent != null && combatEvent.enemyUnitsData != null ? combatEvent.enemyUnitsData.Count : 0;
        Debug.Log($"BattleManager: StartBattle() iniciado. CombatEvent: {combatEventName}. Enemy UnitData recibidos: {enemyDataCount}. Aliados actuales: {playerUnits.Count}.");

        if (playerUnits.Count == 0)
        {
            Debug.LogWarning("BattleManager: StartBattle detecto 0 aliados activos. Intentando GeneratePlayerUnits() automaticamente antes de iniciar el combate.");
            GeneratePlayerUnits();
            Debug.Log($"BattleManager: Regeneracion automatica terminada. Aliados actuales: {playerUnits.Count}.");
        }

        if (combatEvent == null)
        {
            Debug.LogError("BattleManager: StartBattle recibio CombatEvent NULL. No se puede generar enemigos.");
            return;
        }

        GenerateEnemyUnits(combatEvent.enemyUnitsData);
        battleStarted = true;
        SortTurnOrder();
        Debug.Log($"BattleManager: Orden de turnos generado. Aliados: {playerUnits.Count}, enemigos: {enemyUnits.Count}, turnos: {turnOrder.Count}.");
        BattleLoop();
    }
    public void ContinueBattle()
    {        
        SortTurnOrder();
        BattleLoop();
    }
    public void GenerateEnemyUnits(List<UnitData> enemyUnitDataList)
    {
        int receivedCount = enemyUnitDataList != null ? enemyUnitDataList.Count : 0;
        Debug.Log($"BattleManager: GenerateEnemyUnits() iniciado. UnitData enemigos recibidos: {receivedCount}.");
        enemyUnits.Clear();

        if (enemyUnitDataList == null || enemyUnitDataList.Count == 0)
        {
            Debug.LogWarning("BattleManager: enemyUnitDataList is null or empty. No enemy units to generate.");
            return;
        }

        if (enemyUnitsParent == null || enemyUnitsParent.Count == 0)
        {
            Debug.LogError("BattleManager: enemyUnitsParent is not configured. Assign enemy parent transforms in the inspector.");
            return;
        }

        if (!ValidateUnitPrefab())
        {
            return;
        }

        Debug.Log($"BattleManager: enemyUnitsParent asignados: {enemyUnitsParent.Count}.");

        int count = Mathf.Min(enemyUnitDataList.Count, enemyUnitsParent.Count);
        if (count == 0)
        {
            Debug.LogWarning("BattleManager: No enemy units to generate or no enemy unit parent slots assigned.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (enemyUnitDataList[i] == null)
            {
                Debug.LogWarning($"BattleManager: enemyUnitDataList[{i}] is null. Skipping this unit.");
                continue;
            }

            UnitData data = enemyUnitDataList[i];
            Transform parent = enemyUnitsParent[i];
            string unitModelPrefabName = data.unitPrefab != null ? data.unitPrefab.name : "NULL";
            string parentName = parent != null ? parent.name : "NULL";
            Debug.Log($"BattleManager: Preparando enemigo slot {i}. UnitData '{data.unitName}' ({data.name}), unitPrefab: {unitModelPrefabName}, parent: {parentName}.");

            if (data.unitPrefab == null)
            {
                Debug.LogWarning($"BattleManager: Enemigo slot {i} tiene UnitData '{data.name}', pero UnitData.unitPrefab esta NULL. Se instanciara el contenedor Unit, pero no tendra modelo interno.");
            }

            if (parent == null)
            {
                Debug.LogError($"BattleManager: enemyUnitsParent[{i}] is null. Assign all enemy parent transforms in the inspector.");
                continue;
            }

            GameObject unitObj = Instantiate(unitPrefab, parent);
            if (unitObj == null)
            {
                Debug.LogError($"BattleManager: Fallo Instantiate del contenedor unitPrefab para enemigo slot {i}.");
                continue;
            }

            Debug.Log($"BattleManager: Contenedor enemigo instanciado en slot {i}: '{unitObj.name}'.");

            Unit unit = GetUnitComponentFromInstance(unitObj, $"Enemigo slot {i}:");
            if (unit == null)
            {
                Destroy(unitObj);
                continue;
            }

            unit.Initialize(data);
            enemyUnits.Add(unit);
            Debug.Log($"BattleManager: Enemigo slot {i} inicializado correctamente. Enemigos activos: {enemyUnits.Count}.");
        }

        if (enemyUnitDataList.Count > enemyUnitsParent.Count)
        {
            Debug.LogWarning($"BattleManager: enemy unit data list has {enemyUnitDataList.Count} entries but only {enemyUnitsParent.Count} parent slots are assigned. Some units were skipped.");
        }

        Debug.Log($"BattleManager: GenerateEnemyUnits() terminado. Total enemigos instanciados: {enemyUnits.Count}.");
    }


    public void BattleLoop()
    {
        RemoveNullUnits();

        if(playerUnits.Count > 0 && enemyUnits.Count > 0 )
        {
            if(turnOrder.Count > 0)
            {
                ProcessTurn();
            }
            else
            {
                ContinueBattle();
            }          
            
        }
        else
        {
            if(playerUnits.Count == 0)
            {
                Debug.Log("Player Defeated!");
            }
            else
            {
                Debug.Log("Player Victorious!");
                Debug.Log("Player Wins!");
            }

            TryShowReturnButtonAfterBattle();
        }
    }

    private void TryShowReturnButtonAfterBattle()
    {
        if (!battleStarted || battleEnded)
        {
            return;
        }

        battleEnded = true;

        if (showReturnButtonCoroutine != null)
        {
            StopCoroutine(showReturnButtonCoroutine);
        }

        showReturnButtonCoroutine = StartCoroutine(ShowReturnButtonAfterDelay());
    }

    private System.Collections.IEnumerator ShowReturnButtonAfterDelay()
    {
        yield return new WaitForSeconds(returnButtonDelay);

        if (returnToPreviousSceneButton != null)
        {
            returnToPreviousSceneButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("BattleManager: returnToPreviousSceneButton no esta asignado en el inspector.");
        }
    }

    private void HideReturnButton()
    {
        if (showReturnButtonCoroutine != null)
        {
            StopCoroutine(showReturnButtonCoroutine);
            showReturnButtonCoroutine = null;
        }

        if (returnToPreviousSceneButton != null)
        {
            returnToPreviousSceneButton.gameObject.SetActive(false);
        }
    }

    private void LoadPreviousScene()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;

        if (previousSceneIndex < 0)
        {
            Debug.LogWarning("BattleManager: No existe una escena anterior en Build Settings.");
            return;
        }

        SceneManager.LoadScene(previousSceneIndex);
    }

    private void RemoveNullUnits()
    {
        playerUnits.RemoveAll(unit => unit == null);
        enemyUnits.RemoveAll(unit => unit == null);
        turnOrder.RemoveAll(unit => unit == null);
    }
    void ProcessTurn()
    {
            Unit currentUnit = turnOrder[0];
            if (playerUnits.Contains(currentUnit))
            {
                Unit target = null;
                // Player unit's turn
                switch(currentUnit.unitType)
                {
                    case UnitType.AtacaUnidadMasFuerte:
                        target = GetStrongerUnit(enemyUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                enemyUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.AtacaUnidadMasDebil:
                        target = GetWeakerUnit(enemyUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                enemyUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.AtacaUnidadAleatoria:
                        target = GetRandomUnit(enemyUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                enemyUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.SanaUnidadMasDebil:
                         target = GetWeakerUnit(playerUnits);
                         target.TakeDamage(-currentUnit.attackDamage); // Heal by using negative damage
                         
                         turnOrder.RemoveAt(0);

                         BattleLoop();
                        break;
                    case UnitType.SanaUnidadAleatoria:
                         target = GetRandomUnit(playerUnits);
                         target.TakeDamage(-currentUnit.attackDamage); // Heal by using negative damage
                         turnOrder.RemoveAt(0);
                         BattleLoop();
                        break;
                }

            }
            else
            {
                Unit target = null;
                // Enemy unit's turn
                switch(currentUnit.unitType)
                {
                    case UnitType.AtacaUnidadMasFuerte:
                        target = GetStrongerUnit(playerUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                playerUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.AtacaUnidadMasDebil:
                        target = GetWeakerUnit(playerUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                playerUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.AtacaUnidadAleatoria:
                        target = GetRandomUnit(playerUnits);
                        currentUnit.Attack(target, () =>
                        {
                            if (target.currentHealth <= 0)
                            {
                                playerUnits.Remove(target);
                                turnOrder.Remove(target);
                            }
                            turnOrder.RemoveAt(0);
                            BattleLoop();
                        });
                        break;
                    case UnitType.SanaUnidadMasDebil:
                        target = GetWeakerUnit(enemyUnits);
                        target.TakeDamage(-currentUnit.attackDamage); // Heal by using negative damage
                        turnOrder.RemoveAt(0);
                        BattleLoop();
                        break;
                    case UnitType.SanaUnidadAleatoria:
                        target = GetRandomUnit(enemyUnits);
                        target.TakeDamage(-currentUnit.attackDamage); // Heal by using negative damage
                        turnOrder.RemoveAt(0);
                        BattleLoop();
                        break;
                }
            }
    }

    public Unit GetWeakerUnit(List<Unit> units)
    {
        Unit weakerUnit = null;
        float lowestHealth = float.MaxValue;

        foreach (Unit unit in units)
        {
            if (unit.currentHealth < lowestHealth)
            {
                lowestHealth = unit.currentHealth;
                weakerUnit = unit;
            }
        }

        return weakerUnit;
    }
    public Unit GetStrongerUnit(List<Unit> units)
    {
        Unit strongerUnit = null;
        float highestHealth = float.MinValue;

        foreach (Unit unit in units)
        {
            if (unit.currentHealth > highestHealth)
            {
                highestHealth = unit.currentHealth;
                strongerUnit = unit;
            }
        }

        return strongerUnit;
    }
    public Unit GetRandomUnit(List<Unit> units)
    {
        if (units.Count == 0) return null;
        return units[Random.Range(0, units.Count)];
    }
   
}
