using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Combat Event", menuName = "AutoBattler/Combat Event")]
public class CombatEvent : DungeonEvent
{
    public List<UnitData> enemyUnitsData;

    public override void TriggerEvent()
    {    
        BattleManager.Instance.StartBattle(this);
    }
}
