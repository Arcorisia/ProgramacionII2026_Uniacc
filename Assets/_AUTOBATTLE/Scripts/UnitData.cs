using UnityEngine;
[CreateAssetMenu(fileName = "New Unit Data", menuName = "AutoBattler/Unit Data")]
public class UnitData : ScriptableObject
{
    public UnitType unitType;
    public string unitName;
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float defense = 5f;
    public float speed = 1f;
    public bool isPlayerUnit;

    public GameObject unitPrefab;
}

