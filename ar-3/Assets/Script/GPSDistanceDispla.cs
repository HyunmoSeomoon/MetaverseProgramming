using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GPSDistanceDisplay : MonoBehaviour
{
    [Header("UI 설정")]
    public Text[] targetDistanceTexts; // 거리 정보를 표시할 텍스트 배열

    private GPSManager2 gpsManager; // GPSManager2 참조

    private void Start()
    {
        // GPSManager2 인스턴스 가져오기
        gpsManager = GPSManager2.Instance;
        if (gpsManager == null)
        {
            Debug.LogError("GPSManager2를 찾을 수 없습니다.");
            return;
        }

        // 텍스트 배열 크기와 GPS 목표 개수 확인
        if (targetDistanceTexts.Length != gpsManager.targetNames.Length)
        {
            Debug.LogWarning("텍스트 UI 개수와 GPS 목표 개수가 일치하지 않습니다.");
        }

        // 거리 업데이트 주기 설정
        StartCoroutine(UpdatePlayerLocation());
    }

    private IEnumerator UpdatePlayerLocation()
    {
        while (true)
        {
            if (gpsManager != null)
            {
                // 각 목표 지점과의 거리 계산 및 UI 업데이트
                UpdateTargetDistances();
            }
            else
            {
                Debug.LogWarning("GPSManager2 인스턴스가 없습니다.");
            }

            yield return new WaitForSeconds(1f); // 1초마다 업데이트
        }
    }

    private void UpdateTargetDistances()
    {
        for (int i = 0; i < gpsManager.targetNames.Length; i++)
        {
            if (i < targetDistanceTexts.Length)
            {
                // 거리 계산
                double distance = gpsManager.CalculateDistance(
                    gpsManager.playerLat,  // GPSManager2의 playerLat 사용
                    gpsManager.playerLon,  // GPSManager2의 playerLon 사용
                    gpsManager.lats[i],
                    gpsManager.longs[i]
                );

                // 텍스트 업데이트
                targetDistanceTexts[i].text = $"{gpsManager.targetNames[i]}: {distance:F1} m";
            }
            else
            {
                Debug.LogWarning($"텍스트 UI가 부족합니다. 목표 {i + 1}의 거리를 표시할 UI가 없습니다.");
            }
        }
    }
}
