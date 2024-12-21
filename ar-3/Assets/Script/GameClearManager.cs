using UnityEngine;

public class GameClearManager : MonoBehaviour
{
    public PlayerPrefsManager playerPrefsManager;

    public static GameClearManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

        // 특정 목표를 완료했을 때 호출
        public void OnGameClear(string goalID)
    {
        if (playerPrefsManager != null)
        {
            playerPrefsManager.UpdatePlayerProgress(goalID); // 목표 ID를 전달
        }

        Debug.Log($"Game Cleared! Goal {goalID} progress updated.");
        // 추가로 클리어 UI를 활성화하거나 다른 작업 수행 가능
    }
}
