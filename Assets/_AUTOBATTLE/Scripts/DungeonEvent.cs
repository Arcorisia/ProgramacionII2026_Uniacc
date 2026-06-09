using UnityEngine;

public class DungeonEvent : ScriptableObject
{
    public string nameEvent;
    public virtual void TriggerEvent()
    {
        // Base implementation can be empty or contain common logic for all events
    }

}
