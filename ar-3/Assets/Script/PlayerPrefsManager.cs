using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    private const string PlayerProgressKey = "PlayerProgress";

    void Awake()
    {
        InitializePlayerProgress();
    }

    private void InitializePlayerProgress()
    {
        if (!PlayerPrefs.HasKey(PlayerProgressKey))
        {
            PlayerPrefs.SetString(PlayerProgressKey, "");
            PlayerPrefs.Save();
            Debug.Log("Player progress initialized.");
        }
    }

    public void UpdatePlayerProgress(string progress)
    {
        string currentProgress = PlayerPrefs.GetString(PlayerProgressKey, "");
        string updatedProgress = currentProgress + progress;

        PlayerPrefs.SetString(PlayerProgressKey, updatedProgress);
        PlayerPrefs.Save();

        Debug.Log($"Player progress updated: {updatedProgress}");
    }

    public string GetPlayerProgress()
    {
        return PlayerPrefs.GetString(PlayerProgressKey, "");
    }
}
