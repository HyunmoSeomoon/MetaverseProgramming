using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class GPSManager2 : MonoBehaviour
{
    public static GPSManager2 Instance;

    [Header("GPS 설정")]
    public double[] lats; // 목표 지점 위도
    public double[] longs; // 목표 지점 경도
    public string[] targetNames; // 목표 이름 배열

    [Header("거리 감지 설정")]
    public float detectionRange = 100000f; // 목표 지점에 도달했을 때 반경 (미터)

    public double playerLat;
    public double playerLon;
    private int closestTargetIndex = -1; // 가장 가까운 목표 인덱스

    private bool[] isInsideTargetRange; // 각 목표 범위 내에 있는지 추적

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

        isInsideTargetRange = new bool[lats.Length]; // 목표 개수만큼 배열 생성
        for (int i = 0; i < isInsideTargetRange.Length; i++)
        {
            isInsideTargetRange[i] = false; // 초기값은 false
        }
    }

    private bool[] isTargetReached; // 각 목표에 대한 도달 여부를 추적

    private IEnumerator Start()
    {
        // 목표 개수만큼 도달 여부 플래그 초기화
        isTargetReached = new bool[lats.Length];
        for (int i = 0; i < isTargetReached.Length; i++)
        {
            isTargetReached[i] = false; // 초기값은 false
        }

        // 기존 GPS 초기화 로직 유지
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("GPS가 활성화되지 않았습니다.");
            yield break;
        }

        Input.location.Start(10, 1);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("GPS 초기화 실패");
            yield break;
        }

        Debug.Log("GPS 초기화 완료");
        StartCoroutine(UpdatePlayerLocation());
    }

    private IEnumerator UpdatePlayerLocation()
    {
        while (true)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                playerLat = Input.location.lastData.latitude;
                playerLon = Input.location.lastData.longitude;

                UpdateClosestTarget();

                // 가까운 목표에 도달했는지 확인
                if (closestTargetIndex != -1 && IsTargetReached(closestTargetIndex))
                {
                    // 도달하지 않은 목표일 때만 OnTargetReached 호출
                    if (!isTargetReached[closestTargetIndex])
                    {
                        isTargetReached[closestTargetIndex] = true; // 도달 여부 플래그 설정
                        OnTargetReached();
                    }
                }
            }
            else
            {
                Debug.LogWarning("GPS 데이터를 가져올 수 없습니다.");
            }

            yield return new WaitForSeconds(1f);
        }
    }


    private void UpdateClosestTarget()
    {
        double closestDistance = double.MaxValue;
        closestTargetIndex = -1;

        for (int i = 0; i < lats.Length; i++)
        {
            double distance = CalculateDistance(playerLat, playerLon, lats[i], longs[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTargetIndex = i;
            }
        }

        if (closestTargetIndex != -1)
        {
            Debug.Log($"가장 가까운 목표: {targetNames[closestTargetIndex]}, 거리: {closestDistance:F1}m");
        }
    }

    private bool IsTargetReached(int targetIndex)
    {
        double distance = CalculateDistance(playerLat, playerLon, lats[targetIndex], longs[targetIndex]);
        return distance <= 100000f;
    }

    private void OnTargetReached()
    {
        Debug.Log($"목표에 도달했습니다: {targetNames[closestTargetIndex]}");

        // UIManager와 ObjectManager 연동
        UIManager.Instance.ShowWelcomePopUp();
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double theta = lon1 - lon2;
        double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) +
                      Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));

        dist = Math.Acos(dist);
        dist = Rad2Deg(dist);
        dist = dist * 60 * 1.1515 * 1609.344; // 미터 변환

        return dist;
    }

    private double Deg2Rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    private double Rad2Deg(double rad)
    {
        return (rad * 180.0 / Math.PI);
    }

    public Vector3 GPSLocationToWorldPosition(double targetLat, double targetLon)
    {
        const double earthRadius = 6378137; // 지구 반지름 (미터)

        double playerLatRad = playerLat * Mathf.Deg2Rad;
        double targetLatRad = targetLat * Mathf.Deg2Rad;
        double deltaLat = targetLatRad - playerLatRad;
        double deltaLon = (targetLon - playerLon) * Mathf.Deg2Rad;

        double deltaX = earthRadius * deltaLon * Math.Cos(playerLatRad);
        double deltaZ = earthRadius * deltaLat;

        return new Vector3((float)deltaX, 0, (float)deltaZ);
    }
}
