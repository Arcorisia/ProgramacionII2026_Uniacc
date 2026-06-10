using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkinState", menuName = "Mutantasia/Player Skin State")]
public class PlayerSkinState : ScriptableObject
{
    public const string PlayerPrefsKey = "Mutantasia_PlayerSkinActual";

    [Range(0, 10)]
    public int skinPlayerActual;

    public void Load()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            PlayerPrefs.SetInt(PlayerPrefsKey, 0);
            PlayerPrefs.Save();
        }

        skinPlayerActual = Mathf.Clamp(PlayerPrefs.GetInt(PlayerPrefsKey, 0), 0, 10);
    }

    public void Save(int skinIndex)
    {
        skinPlayerActual = Mathf.Clamp(skinIndex, 0, 10);
        PlayerPrefs.SetInt(PlayerPrefsKey, skinPlayerActual);
        PlayerPrefs.Save();
    }
}
