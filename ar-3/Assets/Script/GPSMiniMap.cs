using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class GPSMiniMap : MonoBehaviour
{
    [Header("GPS 설정")]
    public GPSManager gpsManager;  // GPSManager 참조

    [Header("UI 설정")]
    public RectTransform miniMap;        // 미니맵의 RectTransform
    public RectTransform playerIcon;     // 플레이어 아이콘 UI
    public RectTransform targetIcon;     // 목표 오브젝트 아이콘 UI (단일 표시)
    public Text distanceText;            // 가장 가까운 목표 거리 텍스트

    [Header("이미지 설정")]
    public Image miniMapImage;           // MiniMap의 Image 컴포넌트
    public Sprite[] targetSprites;       // 거리 기준으로 바꿀 이미지 스프라이트 배열 (가까울수록 인덱스 0)

    [Header("미니맵 설정")]
    public float mapScale = 800f;        // 미니맵 내 좌표 배율

    private int closestTargetIndex = -1; // 가장 가까운 타겟의 인덱스

    [Header("카메라 설정")]
    public Transform arCamera; // AR 카메라의 Transform

    [Header("UI 토글 설정")]
    public GameObject targetUI;      // 활성화/비활성화할 UI 창
    public Button toggleUIButton;    // UI 토글 버튼

    private void Start()
    {
        if (distanceText != null)
            distanceText.text = "남은 거리: 측정 중...";

        // 버튼 클릭 이벤트 등록
        if (toggleUIButton != null)
        {
            toggleUIButton.onClick.AddListener(ToggleUI);
        }
    }
    // UI 활성화/비활성화 토글
    private void ToggleUI()
    {
        if (targetUI != null)
        {
            bool isActive = targetUI.activeSelf;
            targetUI.SetActive(!isActive);
        }
    }

    private void Update()
    {
        if (gpsManager != null && gpsManager.lats.Length > 0)
        {
            // GPS 플레이어 현재 위치
            double playerLat = Input.location.lastData.latitude;
            double playerLon = Input.location.lastData.longitude;

            // 가장 가까운 타겟 계산
            closestTargetIndex = FindClosestTarget(playerLat, playerLon);

            if (closestTargetIndex != -1)
            {
                double targetLat = gpsManager.lats[closestTargetIndex];
                double targetLon = gpsManager.longs[closestTargetIndex];
                double distance = gpsManager.distance(playerLat, playerLon, targetLat, targetLon);

                // 거리 텍스트 업데이트
                if (distanceText != null)
                {
                    distanceText.text = $"{gpsManager.targetNames[closestTargetIndex]}: {distance:F1} m";
                }

                // 미니맵 아이콘 위치 업데이트
                UpdateTargetIcon();

                // 이미지 변경
                UpdateMiniMapImage();
            }
        }
    }

    private int FindClosestTarget(double playerLat, double playerLon)
    {
        double closestDistance = double.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < gpsManager.lats.Length; i++)
        {
            double distance = gpsManager.distance(playerLat, playerLon, gpsManager.lats[i], gpsManager.longs[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void UpdateTargetIcon()
    {
        if (targetIcon == null || gpsManager == null)
            return;

        // 현재 플레이어의 GPS 좌표
        double playerLat = Input.location.lastData.latitude;
        double playerLon = Input.location.lastData.longitude;

        // 가장 가까운 타겟 찾기
        double minDistance = double.MaxValue;
        int closestTargetIndex = -1;

        for (int i = 0; i < gpsManager.lats.Length; i++)
        {
            double currentDistance = gpsManager.distance(playerLat, playerLon, gpsManager.lats[i], gpsManager.longs[i]);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestTargetIndex = i;
            }
        }

        // 가장 가까운 타겟의 위치를 MiniMap UI 상에 표시
        if (closestTargetIndex >= 0)
        {
            Vector3 targetPosition = GPSLocationToWorldPosition(gpsManager.lats[closestTargetIndex], gpsManager.longs[closestTargetIndex]);
            Vector3 relativePosition = targetPosition - arCamera.transform.position;

            // 미니맵 스케일 적용
            float relativeX = relativePosition.x * mapScale;
            float relativeY = relativePosition.z * mapScale;

            // 타겟 아이콘의 위치를 갱신
            targetIcon.anchoredPosition = new Vector2(relativeX, relativeY);

            // 경계를 벗어나지 않도록 Clamp
            targetIcon.anchoredPosition = Vector2.ClampMagnitude(targetIcon.anchoredPosition, miniMap.sizeDelta.x / 2);

            Debug.Log($"가장 가까운 타겟 아이콘 갱신: {closestTargetIndex}, 거리: {minDistance:F1}m");
        }
    }


    private void UpdateMiniMapImage()
    {
        if (miniMapImage == null || targetSprites == null || targetSprites.Length == 0)
            return;

        // 현재 플레이어의 GPS 좌표
        double playerLat = Input.location.lastData.latitude;
        double playerLon = Input.location.lastData.longitude;

        // 가장 가까운 타겟 찾기
        double minDistance = double.MaxValue;
        int closestTargetIndex = -1;

        for (int i = 0; i < gpsManager.lats.Length; i++)
        {
            double currentDistance = gpsManager.distance(playerLat, playerLon, gpsManager.lats[i], gpsManager.longs[i]);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestTargetIndex = i;
            }
        }

        // 가장 가까운 타겟의 인덱스를 기준으로 이미지 변경
        if (closestTargetIndex >= 0 && closestTargetIndex < targetSprites.Length)
        {
            miniMapImage.sprite = targetSprites[closestTargetIndex];
        }

        Debug.Log($"가장 가까운 타겟: {closestTargetIndex}, 거리: {minDistance:F1}m");
    }

    private Vector3 GPSLocationToWorldPosition(double targetLat, double targetLon)
    {
        const double earthRadius = 6378137; // 지구 반지름 (미터 단위)

        // 현재 플레이어의 GPS 좌표
        double playerLat = Input.location.lastData.latitude;
        double playerLon = Input.location.lastData.longitude;

        double playerLatRad = playerLat * Mathf.Deg2Rad;
        double playerLonRad = playerLon * Mathf.Deg2Rad;

        // 목표 GPS 좌표
        double targetLatRad = targetLat * Mathf.Deg2Rad;
        double targetLonRad = targetLon * Mathf.Deg2Rad;

        // GPS 좌표 차이를 통해 X, Z 거리 계산
        double deltaX = earthRadius * (targetLonRad - playerLonRad) * Mathf.Cos((float)playerLatRad);
        double deltaZ = earthRadius * (targetLatRad - playerLatRad);

        // 월드 좌표 반환 (Z축은 Unity의 전진 방향 기준)
        return new Vector3((float)deltaX, 0, (float)deltaZ);
    }





}
