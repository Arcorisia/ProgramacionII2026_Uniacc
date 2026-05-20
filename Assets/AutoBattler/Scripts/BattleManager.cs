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
        for(int i = 0; i < DataManagerAutoBattler.playerUnits.Count; i++)
        {
            if(DataManagerAutoBattler.playerUnits[i] != null)
            {
                UnitData data = DataManagerAutoBattler.playerUnits[i];
                Transform parent = playerUnitsParent[i];
                GameObject unitObj = Instantiate(unitPrefab, parent);
                Unit unit = unitObj.GetComponent<Unit>();
                unit.Initialize(data);
                playerUnits.Add(unit);
            }
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
        for(int i = 0; i < enemyUnitDataList.Count; i++)
        {
            UnitData data = enemyUnitDataList[i];
            Transform parent = enemyUnitsParent[i];
            GameObject unitObj = Instantiate(unitPrefab, parent);
            Unit unit = unitObj.GetComponent<Unit>();
            unit.Initialize(data);
            enemyUnits.Add(unit);
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
