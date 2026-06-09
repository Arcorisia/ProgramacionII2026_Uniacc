using UnityEngine;
using System.Collections.Generic;
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }
    public List<DungeonEvent> dungeonEvents;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        StartDungeon();
    }
    public void StartDungeon()
    {
        BattleManager.Instance.GeneratePlayerUnits();
         if (dungeonEvents.Count > 0)
        {
            DungeonEvent currentEvent = dungeonEvents[0];
            currentEvent.TriggerEvent();
            dungeonEvents.RemoveAt(0);
        }        
    }
    public void ContinueDungeon()
    {
        if (dungeonEvents.Count > 0)
        {
            DungeonEvent currentEvent = dungeonEvents[0];
            currentEvent.TriggerEvent();
            dungeonEvents.RemoveAt(0);
        }
        else
        {
            Debug.Log("Dungeon Completed!");
        }
        
    }
}
