using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }        
    }
    private void SortTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(playerUnits);
        turnOrder.AddRange(enemyUnits);
        turnOrder.Sort((a, b) => b.speed.CompareTo(a.speed));
    }
    
    public void GeneratePlayerUnits()
    {
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

        int count = Mathf.Min(DataManagerAutoBattler.playerUnits.Count, playerUnitsParent.Count);
        if (count == 0)
        {
            Debug.LogWarning("BattleManager: No player units to generate or no player unit parent slots assigned.");
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError("BattleManager: unitPrefab is not assigned. Assign the unit prefab in the inspector.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (DataManagerAutoBattler.playerUnits[i] != null)
            {
                UnitData data = DataManagerAutoBattler.playerUnits[i];
                Transform parent = playerUnitsParent[i];

                if (parent == null)
                {
                    Debug.LogError($"BattleManager: playerUnitsParent[{i}] is null. Assign all parent transforms in the inspector.");
                    continue;
                }

                GameObject unitObj = Instantiate(unitPrefab, parent);
                if (unitObj == null)
                {
                    Debug.LogError("BattleManager: Failed to instantiate unit prefab.");
                    continue;
                }

                Unit unit = unitObj.GetComponent<Unit>();
                if (unit == null)
                {
                    Debug.LogError("BattleManager: unitPrefab does not contain a Unit component.");
                    Destroy(unitObj);
                    continue;
                }

                unit.Initialize(data);
                playerUnits.Add(unit);
            }
        }

        if (DataManagerAutoBattler.playerUnits.Count > playerUnitsParent.Count)
        {
            Debug.LogWarning($"BattleManager: playerUnits list has {DataManagerAutoBattler.playerUnits.Count} entries but only {playerUnitsParent.Count} parent slots are assigned. Some units were skipped.");
        }
    }
    public void StartBattle(CombatEvent combatEvent)
    {
        //GeneratePlayerUnits();
        GenerateEnemyUnits(combatEvent.enemyUnitsData);
        SortTurnOrder();
        BattleLoop();
    }
    public void ContinueBattle()
    {        
        SortTurnOrder();
        BattleLoop();
    }
    public void GenerateEnemyUnits(List<UnitData> enemyUnitDataList)
    {
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

        if (unitPrefab == null)
        {
            Debug.LogError("BattleManager: unitPrefab is not assigned. Assign the unit prefab in the inspector.");
            return;
        }

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

            if (parent == null)
            {
                Debug.LogError($"BattleManager: enemyUnitsParent[{i}] is null. Assign all enemy parent transforms in the inspector.");
                continue;
            }

            GameObject unitObj = Instantiate(unitPrefab, parent);
            if (unitObj == null)
            {
                Debug.LogError("BattleManager: Failed to instantiate enemy unit prefab.");
                continue;
            }

            Unit unit = unitObj.GetComponent<Unit>();
            if (unit == null)
            {
                Debug.LogError("BattleManager: unitPrefab does not contain a Unit component.");
                Destroy(unitObj);
                continue;
            }

            unit.Initialize(data);
            enemyUnits.Add(unit);
        }

        if (enemyUnitDataList.Count > enemyUnitsParent.Count)
        {
            Debug.LogWarning($"BattleManager: enemy unit data list has {enemyUnitDataList.Count} entries but only {enemyUnitsParent.Count} parent slots are assigned. Some units were skipped.");
        }
    }


    public void BattleLoop()
    {
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
                DungeonManager.Instance.ContinueDungeon();
                return;
            }
        }
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
