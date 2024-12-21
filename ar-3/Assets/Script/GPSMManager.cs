using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class GPSManager : MonoBehaviour
{
    public Text text_ui;
    public GameObject welcome_popUp;
    public Button welcomeButton; // welcome_popUp의 버튼
    public bool isFirst = false;

    [Header("UI 설정")]
    //public Text text_ui; // 전체 거리 표시
    public Text[] targetDistanceTexts; // 각 목표의 거리 텍스트 배열


    public double[] lats; // 목표 지점 위도
    public double[] longs; // 목표 지점 경도
    public string[] targetNames; // 목표 이름 배열

    private double playerLat;
    private double playerLon;

    public GameObject objectToSpawn; // 첫 번째 오브젝트(알)
    public GameObject secondaryUI; // 첫 번째 UI
    public Button secondaryUIButton; // 첫 번째 UI의 버튼

    public GameObject nextObject; // 두 번째 오브젝트
    public GameObject[] nextObjects; // 3개의 오브젝트
    public float nextObjectSpawnDistance = 6f; // 두 번째 오브젝트 생성 거리

    public GameObject bugObject; // 벌레 오브젝트
    public float bugSpawnDistance =20f; // 벌레 생성 거리
    public float bugFollowSpeed = 1f; // 벌레 이동 속도

    public GameObject finalUI; // 최종 UI
    public GameObject failureUI; // 실패 UI
    public Camera arCamera; // AR 카메라

    private bool isFinalUIActive = false; // 최종 UI 활성화 여부
    private bool isBugFollowing = false; // 벌레 이동 활성화 여부
    private bool isFailureActive = false; // 실패 UI 활성화 여부

    public float spawnDistance = 0.5f; // 첫 번째 오브젝트(알)의 카메라 앞쪽 거리
    public float detectionRange = 80000000000000f; // 특정 거리 안에 있을 때 welcome_popUp 표시


    private float elapsedTime = 0f; // 경과 시간
    private bool isTimerRunning = false; // 타이머 실행 여부
    public Text timerText; // 경과 시간을 표시할 텍스트


    public AudioSource audioSource; // AudioSource 컴포넌트
    public AudioClip initialClip; // 처음 시작할 때 재생할 노래
    public AudioClip secondaryClip; // secondaryUIButton 클릭 시 재생할 노래


    public GameObject fixedObject;
    public Vector3 fixedOffset = new Vector3(0, -0.5f, 0.5f);

    IEnumerator Start()
    {
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        if (!Input.location.isEnabledByUser)
            yield break;

        Input.location.Start(10, 1);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }





        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            print("Location initialized.");


            // 처음 노래 재생
            if (audioSource != null && initialClip != null)
            {
                audioSource.clip = initialClip;
                audioSource.Play();
            }

            /*while (true)
            {
                yield return null;

                // GPS 좌표 업데이트
                double myLat = Input.location.lastData.latitude;
                double myLong = Input.location.lastData.longitude;

                // 목표 지점까지의 거리 계산
                double remainDistance = distance(myLat, myLong, lats[0], longs[0]);

                text_ui.text = $"Distance: {remainDistance:F1} m";

                // 특정 거리 안에 있으면 welcome_popUp 활성화
                if (remainDistance <= detectionRange && !isFirst)
                {
                    isFirst = true;
                    welcome_popUp.SetActive(true);

                    // welcomeButton 클릭 이벤트 연결
                    if (welcomeButton != null)
                    {
                        welcomeButton.onClick.AddListener(() =>
                        {
                            StartCoroutine(SpawnObjectAndUI(4f)); // 버튼 클릭 후 4초 후 알과 UI 활성화
                            welcome_popUp.SetActive(false); // welcome_popUp 비활성화
                        });
                    }
                }
            }*/
            while (true)
            {
                yield return null;

                // GPS 좌표 업데이트
                double myLat = Input.location.lastData.latitude;
                double myLong = Input.location.lastData.longitude;

                // 모든 목표 지점과의 거리 계산 및 UI 업데이트
                for (int i = 0; i < lats.Length; i++)
                {
                    double remainDistance = distance(myLat, myLong, lats[i], longs[i]);

                    // 거리 및 목표 이름을 텍스트 UI에 표시
                    if (targetDistanceTexts != null && i < targetDistanceTexts.Length)
                    {
                        targetDistanceTexts[i].text = $"{targetNames[i]}: {remainDistance:F1} m 남음";
                    }
                }

                // 첫 번째 목표에 도달했을 때 팝업 활성화 (예시)
                double firstTargetDistance = distance(myLat, myLong, lats[0], longs[0]);
                if (firstTargetDistance <= detectionRange && !isFirst)
                {
                    isFirst = true;
                    welcome_popUp.SetActive(true);

                    // welcomeButton 클릭 이벤트 연결
                    if (welcomeButton != null)
                    {
                        welcomeButton.onClick.AddListener(() =>
                        {
                            StartCoroutine(SpawnObjectAndUI(4f)); // 버튼 클릭 후 4초 후 알과 UI 활성화
                            welcome_popUp.SetActive(false); // welcome_popUp 비활성화
                        });
                    }
                }
            }
        }
    }

    void Update()
    {

        // fixedObject 고정 위치 유지 및 nextObject 방향 바라보기
        if (fixedObject != null && fixedObject.activeSelf)
        {
            // 카메라 앞 오프셋 위치 유지
            fixedObject.transform.position = arCamera.transform.position + arCamera.transform.forward * fixedOffset.z + arCamera.transform.up * fixedOffset.y;

            // nextObject의 방향을 Y축 기준으로 바라보기
            if (nextObject != null && nextObject.activeSelf)
            {
                Vector3 targetDirection = nextObject.transform.position - fixedObject.transform.position;
                targetDirection.y = 0; // Y축만 회전하도록 X, Z 방향으로만 회전 계산
                if (targetDirection != Vector3.zero)
                {
                    fixedObject.transform.rotation = Quaternion.LookRotation(targetDirection);
                }
            }
        }
        if (Input.location.status == LocationServiceStatus.Running)
        {
            playerLat = Input.location.lastData.latitude;
            playerLon = Input.location.lastData.longitude;
        }
        // 타이머 업데이트
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = $"Time: {elapsedTime:F2} s";
        }

        // 두 번째 오브젝트와 카메라 거리 계산
        if (nextObject != null && nextObject.activeSelf && !isFinalUIActive)
        {
            float distanceToCamera = Vector3.Distance(arCamera.transform.position, nextObject.transform.position);
            if (distanceToCamera < 0.5f)
            {
                finalUI.SetActive(true);
                isFinalUIActive = true;
            }
        }

        // 벌레 오브젝트 따라오기 로직
        if (isBugFollowing && bugObject != null)
        {
            Vector3 targetPosition = arCamera.transform.position;
            bugObject.transform.position = Vector3.MoveTowards(bugObject.transform.position, targetPosition, bugFollowSpeed * Time.deltaTime);

            // 카메라와 벌레의 거리 계산
            float distanceToCamera = Vector3.Distance(arCamera.transform.position, bugObject.transform.position);
            if (distanceToCamera < 0.3f && !isFailureActive)
            {
                failureUI.SetActive(true);
                isFailureActive = true;
                isBugFollowing = false; // 따라오기를 멈춤
                StopTimer(); // 타이머 멈춤
            }
        }
    }

    // GPS 데이터를 월드 좌표로 변환하는 메서드
    private Vector3 GPSLocationToWorldPosition(double targetLat, double targetLon)
    {
        const double earthRadius = 6378137; // 지구 반지름 (미터 단위)

        // GPS 좌표를 라디안으로 변환
        double playerLatRad = playerLat * Mathf.Deg2Rad;
        double targetLatRad = targetLat * Mathf.Deg2Rad;

        // 차이 계산
        double deltaLat = targetLatRad - playerLatRad;
        double deltaLon = (targetLon - playerLon) * Mathf.Deg2Rad;

        // 상대적 위치 계산
        double deltaX = earthRadius * deltaLon * Mathf.Cos((float)playerLatRad);
        double deltaZ = earthRadius * deltaLat;

        // 상대적 위치 반환 (Unity는 Z가 전진 방향)
        return new Vector3((float)deltaX, 0, (float)deltaZ);
    }


    // 지표면 거리 계산 공식(하버사인 공식)
    public double distance(double lat1, double lon1, double lat2, double lon2)
    {
        double theta = lon1 - lon2;

        double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));

        dist = Math.Acos(dist);

        dist = Rad2Deg(dist);

        dist = dist * 60 * 1.1515;

        dist = dist * 1609.344; // 미터 변환

        return dist;
    }

    private double Deg2Rad(double deg)
    {
        return (deg * Mathf.PI / 180.0f);
    }

    private double Rad2Deg(double rad)
    {
        return (rad * 180.0f / Mathf.PI);
    }

    // 4초 후 알과 UI 활성화
    private IEnumerator SpawnObjectAndUI(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (objectToSpawn != null && arCamera != null)
        {
            // 알 오브젝트 생성
            Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.forward * spawnDistance;
            objectToSpawn.transform.position = spawnPosition;
            objectToSpawn.SetActive(true);

            // UI 활성화
            if (secondaryUI != null)
            {
                secondaryUI.SetActive(true);
            }

            // 버튼 클릭 이벤트 연결
            if (secondaryUIButton != null)
            {
                secondaryUIButton.onClick.AddListener(OnSecondaryUIButtonClicked);
            }
        }
    }

    // 첫 번째 UI 버튼 클릭 시 실행
    /*private void OnSecondaryUIButtonClicked()
    {
        // 알과 UI 비활성화
        if (objectToSpawn != null)
        {
            objectToSpawn.SetActive(false);
        }
        if (secondaryUI != null)
        {
            secondaryUI.SetActive(false);
        }

        // 두 번째 오브젝트 카메라 앞 생성
        if (nextObject != null && arCamera != null)
        {
            Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.forward * nextObjectSpawnDistance*6;
            Debug.Log($"카메라 위치: {arCamera.transform.position}, 생성 위치: {spawnPosition}");
            nextObject.transform.position = spawnPosition;
            nextObject.SetActive(true);
        }

        // 벌레 오브젝트 카메라 뒤 생성
        if (bugObject != null && arCamera != null)
        {
            Vector3 spawnPosition = arCamera.transform.position - arCamera.transform.forward * bugSpawnDistance*10;
            bugObject.transform.position = spawnPosition;
            bugObject.SetActive(true);

            // 10초 후 따라오기 시작
            StartCoroutine(ActivateBugFollow(10f));
        }

        // 특정 오브젝트 활성화
        if (fixedObject != null)
        {
          //  fixedObject.SetActive(true);
        }

        // 다른 노래 재생
        if (audioSource != null && secondaryClip != null)
        {
            audioSource.clip = secondaryClip;
            audioSource.Play();
        }

        // 타이머 시작
        StartTimer();
    }*/

    // 첫 번째 UI 버튼 클릭 시 실행
    private void OnSecondaryUIButtonClicked()
    {
        // 알과 UI 비활성화
        if (objectToSpawn != null)
        {
            objectToSpawn.SetActive(false);
        }
        if (secondaryUI != null)
        {
            secondaryUI.SetActive(false);
        }

        // 각 오브젝트를 경도와 위도에 따라 생성
        for (int i = 0; i < nextObjects.Length && i < lats.Length && i < longs.Length; i++)
        {
            if (nextObjects[i] != null)
            {
                // 경도와 위도를 월드 좌표로 변환
                Vector3 spawnPosition = GPSLocationToWorldPosition(lats[i],longs[i]);


                // 오브젝트 위치 설정 및 활성화
                nextObjects[i].transform.position = spawnPosition;
                nextObjects[i].SetActive(true);

                Debug.Log($"오브젝트 {i + 1} 위치: {spawnPosition}");
            }
        }

        // 타이머 시작
        StartTimer();
       // StartCoroutine(ActivateBugFollow(3));

        // 다른 노래 재생
        if (audioSource != null && secondaryClip != null)
        {
            audioSource.clip = secondaryClip;
            audioSource.Play();
        }

        // 특정 오브젝트 활성화
        if (fixedObject != null)
        {
            //  fixedObject.SetActive(true);
        }

        if (bugObject != null && arCamera != null)
        {
            Vector3 spawnPosition = arCamera.transform.position - arCamera.transform.forward * bugSpawnDistance * 10;
            bugObject.transform.position = spawnPosition;
            bugObject.SetActive(true);

            // 10초 후 따라오기 시작
            StartCoroutine(ActivateBugFollow(10f));
        }
    }

    private void StartTimer()
    {
        elapsedTime = 0f; // 경과 시간 초기화
        isTimerRunning = true; // 타이머 시작
    }

    private void StopTimer()
    {
        isTimerRunning = false; // 타이머 중지
    }

    // 10초 후 벌레가 따라오기를 활성화
    private IEnumerator ActivateBugFollow(float delay)
    {
        yield return new WaitForSeconds(delay);
        isBugFollowing = true;
    }
}
