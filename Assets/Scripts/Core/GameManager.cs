using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    
    public SubtoneBarManager subtoneBar;
    public static GameManager instance;
    public int coinCount = 0;
    public TextMeshProUGUI coinText;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;           
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void AddCoins(int amount)
        {
            coinCount += amount;

            subtoneBar.OnCoinsChanged(coinCount);
        }
    void Start()
    {
        coinText.text = "x" + coinCount;
    }
    public void CollectCoin(int amount)
    {
        coinCount += amount;
        coinText.text = "x" + coinCount;
        Debug.Log("Coins: " + coinCount);
    }
}
