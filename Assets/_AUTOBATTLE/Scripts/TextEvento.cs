using UnityEngine;
using DG.Tweening;
[CreateAssetMenu(fileName = "New Text Event", menuName = "AutoBattler/Text Event")]
public class TextEvento : DungeonEvent
{
    public string textToDisplay;

    public override void TriggerEvent()
    {
        // Display the text to the player (you can replace this with your own UI logic)
        Debug.Log(textToDisplay);
        // Optionally, you can add a delay before continuing to the next event
        DOTween.Sequence()
            .AppendInterval(2f) // Wait for 2 seconds
            .AppendCallback(() => DungeonManager.Instance.ContinueDungeon()); // Continue to the next event
    }
    
}
