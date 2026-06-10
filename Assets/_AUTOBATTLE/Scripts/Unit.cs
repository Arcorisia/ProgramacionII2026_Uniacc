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
        TMP_Text damageText = null;
        GameObject damageTextObj = null;

        if (BattleManager.Instance != null && BattleManager.Instance.damageTextPrefab != null)
        {
            damageTextObj = Instantiate(BattleManager.Instance.damageTextPrefab,
                transform.position + new Vector3(0, 2, 0), Quaternion.identity);
            damageText = damageTextObj.GetComponentInChildren<TMP_Text>(true);

            if (damageTextObj != null)
            {
                damageTextObj.transform.DOJump(damageTextObj.transform.position, 1f, 1, 0.5f).
                    SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    Destroy(damageTextObj);
                });
            }
        }
        else
        {
            Debug.LogError("Unit: BattleManager.damageTextPrefab no esta asignado. Asigna un prefab de texto de dano en BattleManager.");
        }

        if (damageTextObj != null && damageText == null)
        {
            Debug.LogError($"Unit: El Damage Text Prefab '{damageTextObj.name}' no tiene componente TMP_Text/TextMeshPro ni en la raiz ni en hijos.");
        }

        if(damage >= 0)
        {
            effectiveDamage = Mathf.Max(damage - defense, 0);           
            if (damageText != null)
            {
                damageText.text = effectiveDamage.ToString("0");
                damageText.color = Color.red;
            }
        }
        else
        {
            effectiveDamage = damage; // Healing is not reduced by defense
            if (damageText != null)
            {
                damageText.text = effectiveDamage.ToString("0");
                damageText.color = Color.green;
            }
        }

        
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (currentHealth - effectiveDamage) / maxHealth;
        }
        else
        {
            Debug.LogError($"Unit: healthBarFill no esta asignado en '{name}'. Asigna la imagen Fill de la barra de vida en el prefab BattleUnit.");
        }

        currentHealth -= effectiveDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (damageTween != null)
            {
                damageTween.Restart();
            }
        }
    }
    void Die()
    {
        // Handle unit death (e.g., play animation, remove from game)
        Destroy(gameObject);
    }
}
