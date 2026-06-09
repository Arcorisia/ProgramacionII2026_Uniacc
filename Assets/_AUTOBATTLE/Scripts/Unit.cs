using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Unit : MonoBehaviour
{
    public UnitType unitType;
   
    public float maxHealth = 100f;
    public float currentHealth;
    public float attackDamage = 10f;
    public float defense = 5f;
    public float speed = 1f;
    public Transform frontPoint;
    Tween damageTween;
    public Image healthBarFill;
    public UnitData data;
    public Transform modelParent;
    private GameObject modelInstance;

    public void Initialize(UnitData unitData)
    {
        data = unitData;
        maxHealth = unitData.maxHealth;
        attackDamage = unitData.attackDamage;
        defense = unitData.defense;
        speed = unitData.speed;
        unitType = unitData.unitType;

        if (unitData.unitPrefab != null)
        {
            if (modelInstance != null)
                Destroy(modelInstance);

            modelInstance = Instantiate(unitData.unitPrefab, modelParent != null ? modelParent : transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;

            if (!data.isPlayerUnit)
            {
                modelInstance.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            }
        }
    }

    void Start()
    {
        Initialize(data);
        damageTween = transform.DOPunchScale(new Vector3(.2f,- .2f, .2f), 0.2f).SetAutoKill(false).Pause();
        currentHealth = maxHealth;
    }
    public void Attack(Unit target, System.Action onAttackComplete = null)
    {
        Vector3 originalPosition = transform.position;
        transform.DOJump(target.frontPoint.position, 1f, 1, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
             target.TakeDamage(attackDamage);
             transform.DOMove(originalPosition, 0.5f).SetEase(Ease.InQuad).OnComplete(() =>
             {
                 onAttackComplete?.Invoke();
             });
        });
       
    }
    public void TakeDamage(float damage)
    {
        float effectiveDamage;
        GameObject damageTextObj = Instantiate(BattleManager.Instance.damageTextPrefab, 
            transform.position + new Vector3(0,2,0), Quaternion.identity);
        TextMeshPro damageText = damageTextObj.GetComponent<TextMeshPro>();
        damageTextObj.transform.DOJump(damageTextObj.transform.position, 1f, 1, 0.5f).
            SetEase(Ease.OutQuad).OnComplete(() =>
        {
            Destroy(damageTextObj);
        });
        if(damage >= 0)
        {
            effectiveDamage = Mathf.Max(damage - defense, 0);           
            damageText.text = effectiveDamage.ToString("0");
            damageText.color = Color.red;
        }
        else
        {
            effectiveDamage = damage; // Healing is not reduced by defense
            damageText.text = effectiveDamage.ToString("0");
            damageText.color = Color.green;
        }

        
        healthBarFill.fillAmount = (currentHealth - effectiveDamage) / maxHealth;        
        currentHealth -= effectiveDamage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            damageTween.Restart();
        }
    }
    void Die()
    {
        // Handle unit death (e.g., play animation, remove from game)
        Destroy(gameObject);
    }
}
