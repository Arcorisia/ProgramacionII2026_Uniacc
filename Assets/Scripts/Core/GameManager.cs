using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public SubtoneBarManager subtoneBar;
    public int coinCount;

    public static GameManager instance;

    public TextMeshProUGUI coinText;   // Texto de esporas
    public TextMeshProUGUI romanText;  // 👈 NUEVO texto para contador romano

    private int romanCounter = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        coinText.text = "Esporas: " + coinCount;

        // Iniciar contador
        StartCoroutine(RomanCounterRoutine());
    }

    public void AddCoins(int amount)
    {
        coinCount += amount;
    }

    public void CollectCoin(int amount)
    {
        coinCount += amount;
        coinText.text = "Esporas: " + coinCount;
        Debug.Log("Esporas: " + coinCount);
    }

    IEnumerator RomanCounterRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            string roman = ToRoman(romanCounter);
            romanText.text = roman; // 👈 ahora usa SU propio texto

            romanCounter++;
        }
    }

    string ToRoman(int number)
    {
        string[] thousands = { "", "M", "MM", "MMM" };
        string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        return thousands[number / 1000] +
               hundreds[(number % 1000) / 100] +
               tens[(number % 100) / 10] +
               ones[number % 10];
    }
}