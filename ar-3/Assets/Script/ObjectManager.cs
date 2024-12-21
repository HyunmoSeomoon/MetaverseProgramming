using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ObjectManager : MonoBehaviour
{
    public static ObjectManager Instance;

    [Header("오브젝트 설정")]
    public GameObject objectToSpawn; // 첫 번째 알 오브젝트
    public GameObject bugObject;    // 벌레 오브젝트
    public Transform playerTransform; // 플레이어 Transform


    [Header("벌레 소리 설정")]
    public AudioSource bugAudioSource; // 벌레 소리 재생을 위한 AudioSource
    public float bugSoundDistance = 10f; // 벌레 소리가 들리기 시작하는 거리
    public AudioClip bugSound;

    [Header("벌레 설정")]
    public float bugFollowDelay = 2f; // 벌레 쫓아오기 시작 딜레이
    public float bugFollowSpeed = 3f; // 벌레 이동 속도
    public float bugDetectionDistance = 0.3f; // 벌레가 플레이어를 잡는 거리

    private bool isBugFollowing = false; // 벌레 따라오기 활성화 여부
    private bool isBugActionTriggered = false; // 벌레가 플레이어를 잡았는지 여부

    [Header("UI 연동")]
    public GameObject failureUI; // 벌레에 잡혔을 때 표시할 UI
    public GameObject clearUI; // 성공 시 표시할 UI
   


    [Header("Next Objects")]
    //public GameObject[] nextObjects; // 여러 목표 오브젝트
    [Header("Goal Objects")]
    public GameObject goalObject1;
    public GameObject goalObject2;
    public GameObject goalObject3;
    public GameObject goalObject4;
    public GameObject goalObject5;


    [Header("각 목표 UI")]
    public GameObject goal1UI;
    public GameObject goal2UI;
    public GameObject goal3UI;
    public GameObject goal4UI;
    public GameObject goal5UI;

    public float spawnDistanceFromTarget = 6f;
    public float bugSpawnDistance = 1f;
    public float goalObjectDistance = 8f; // 목표 오브젝트에 도달할 거리

    private bool isGameActive = true; // 게임 활성화 여부

    public Text 벌레거리;

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

    /// <summary>
    /// 알 오브젝트를 플레이어 근처에 스폰.
    /// </summary>
    public void SpawnObjectAtTarget()
    {
        if (objectToSpawn != null && playerTransform != null)
        {
            Vector3 spawnPosition = playerTransform.position + playerTransform.forward * 2f; // 플레이어 앞 2m
            objectToSpawn.transform.position = spawnPosition;
            objectToSpawn.SetActive(true);
            Debug.Log("알 오브젝트 스폰 완료.");
        }
    }

    public void SpawnGoalObjects(Vector3 goalPosition1, Vector3 goalPosition2, Vector3 goalPosition3)
    {
        float yOffset = -2f; // y축으로 낮출 오프셋 값
        if (goalObject1 != null)
        {
            goalPosition1.y += yOffset;
            // 앵커 생성 후 오브젝트 위치 설정
            AnchorManager.Instance.CreateAnchor(goalPosition1, Quaternion.identity);
            goalObject1.transform.position = goalPosition1;
            goalObject1.SetActive(true);
            Debug.Log($"GoalObject1 생성 위치: {goalPosition1}");
        }

        if (goalObject2 != null)
        {
            goalPosition2.y += yOffset;
            // 앵커 생성 후 오브젝트 위치 설정
            AnchorManager.Instance.CreateAnchor(goalPosition2, Quaternion.identity);
            goalObject2.transform.position = goalPosition2;
            goalObject2.SetActive(true);
            Debug.Log($"GoalObject2 생성 위치: {goalPosition2}");
        }

        if (goalObject3 != null)
        {
            goalPosition3.y += yOffset;
            // 앵커 생성 후 오브젝트 위치 설정
            AnchorManager.Instance.CreateAnchor(goalPosition3, Quaternion.identity);
            goalObject3.transform.position = goalPosition3;
            goalObject3.SetActive(true);
            Debug.Log($"GoalObject3 생성 위치: {goalPosition3}");
        }
        if (goalObject4 != null)
        {
            goalPosition3.y += yOffset;
            // 앵커 생성 후 오브젝트 위치 설정
            AnchorManager.Instance.CreateAnchor(goalPosition3, Quaternion.identity);
            goalObject4.transform.position = goalPosition3;
            goalObject4.SetActive(true);
            Debug.Log($"GoalObject3 생성 위치: {goalPosition3}");
        }
        if (goalObject5 != null)
        {
            goalPosition3.y += yOffset;
            // 앵커 생성 후 오브젝트 위치 설정
            AnchorManager.Instance.CreateAnchor(goalPosition3, Quaternion.identity);
            goalObject5.transform.position = goalPosition3;
            goalObject5.SetActive(true);
            Debug.Log($"GoalObject3 생성 위치: {goalPosition3}");
        }
    }


    /// <summary>
    /// 벌레 따라오기 시작.
    /// </summary>
    public void StartBugFollow()
    {
        if (bugObject != null && playerTransform != null)
        {
            // 벌레를 플레이어와 일정 거리 떨어진 곳에 생성
            Vector3 spawnPosition = playerTransform.position - playerTransform.forward * bugSpawnDistance*20;
            bugObject.transform.position = spawnPosition;
            bugObject.SetActive(true);
            //isBugFollowing = true;
            // 벌레 따라오기 시작
            StartCoroutine(StartBugFollowRoutine());
        }
        else
        {
            벌레거리.text = playerTransform == null
                ? "플레이어가 설정되지 않았습니다!"
                : "벌레 오브젝트가 설정되지 않았습니다!";
        }
    }

    /// <summary>
    /// 벌레 따라오기를 딜레이 후 시작.
    /// </summary>
    private IEnumerator StartBugFollowRoutine()
    {
        yield return new WaitForSeconds(bugFollowDelay);

        if (bugObject != null && playerTransform != null)
        {
            isBugFollowing = true;
            Debug.Log("벌레 따라오기 시작.");
        }
    }




    private void Update()
    {
        if (!isGameActive)
            return;
        if (isBugFollowing && bugObject != null && playerTransform != null)
        {
            // 벌레가 플레이어를 따라감
            bugObject.transform.position = Vector3.MoveTowards(
                bugObject.transform.position,
                playerTransform.position,
                bugFollowSpeed * Time.deltaTime
            );

            // 벌레와 플레이어 간 거리 확인
            float distanceToPlayer = Vector3.Distance(bugObject.transform.position, playerTransform.position);
            //벌레거리.text = $"벌레와의 거리: if (distanceToPlayer <= bugSoundDistance)
            if (distanceToPlayer <= 10)
            {
                PlayBugSound();
            }

            if (distanceToPlayer <= bugDetectionDistance && !isBugActionTriggered)
            {
                isBugActionTriggered = true;
                OnBugCaughtPlayer();
            }
        }
        CheckNextObjectProximity();
    }
    private void PlayBugSound()
    {
        if (bugAudioSource != null && !bugAudioSource.isPlaying)
        {
            // bugSound.clip = 
            bugAudioSource.clip = bugSound;
            bugAudioSource.Play();
            Debug.Log("벌레 소리 재생 중...");
        }
    }
    /// <summary>
    /// 벌레가 플레이어를 잡았을 때 호출되는 메서드.
    /// </summary>
    private void OnBugCaughtPlayer()
    {
        Debug.Log("벌레가 플레이어를 잡았습니다!");
        if (bugAudioSource != null && bugAudioSource.isPlaying)
        {
            bugAudioSource.Stop();
            Debug.Log("벌레 소리 중지.");
        }
        // 벌레 따라오기를 멈춤
        isBugFollowing = false;
        isGameActive = false;
        // 실패 UI 활성화
        if (failureUI != null)
        {
            failureUI.SetActive(true);
        }
        // 시간 멈춤
        Time.timeScale = 0;
    }

    public void CheckNextObjectProximity()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player transform is not assigned.");
            return;
        }

        // 가장 가까운 목표 오브젝트 식별
        GameObject closestGoal = GetClosestGoalObject();
        if (closestGoal != null)
        {
            CheckProximityForGoal(closestGoal, closestGoal.name);
        }
    }


    void CheckProximityForGoal(GameObject goalObject, string goalName)
    {
        if (goalObject != null && goalObject.activeSelf)
        {
            float distanceToGoal = Vector3.Distance(playerTransform.position, goalObject.transform.position);
            벌레거리.text = $"용암: {distanceToGoal:F2} m";
            // 특정 거리 안에 들어오면 처리
            if (distanceToGoal <= 7)//goalObjectDistance
            {
                벌레거리.text = $"도착!!!";
                Debug.Log($"{goalName} reached!");
                OnGoalReached(goalObject);
              //  ClearTime.text = $"완료시간: {distanceToGoal:F2} m";
            }
        }
    }
    private void OnGoalReached(GameObject goalObject)
    {
        Debug.Log($"Reached goal: {goalObject.name}");

        // 목표 오브젝트 비활성화
        goalObject.SetActive(false);
        clearUI.SetActive(true);
        isGameActive = false;
        // 시간 멈춤
        Time.timeScale = 0;
        // 클리어 UI 표시 또는 추가 로직
        //UIManager.Instance.ShowClearUI(); // 예시

        string goalID = GetGoalID(goalObject);

        if (GameClearManager.Instance != null)
        {
            GameClearManager.Instance.OnGameClear(goalID);
        }

        if (goalObject == goalObject1)
        {
            goal1UI.SetActive(true);
        }
        else if (goalObject == goalObject2)
        {
            goal2UI.SetActive(true);
        }
        else if (goalObject == goalObject3)
        {
            goal3UI.SetActive(true);
        }
        else if (goalObject == goalObject4)
        {
            goal4UI.SetActive(true);
        }
        else if (goalObject == goalObject5)
        {
            goal5UI.SetActive(true);
        }
    }
    private string GetGoalID(GameObject goalObject)
    {
        if (goalObject == goalObject1) return "1";
        if (goalObject == goalObject2) return "2";
        if (goalObject == goalObject3) return "3";
        if (goalObject == goalObject4) return "4";
        if (goalObject == goalObject5) return "5";
        return ""; // 예외 처리
    }


    public void SpawnObjectsAlongPath(GameObject goalObject, GameObject prefabToSpawn)
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player transform이 설정되지 않았습니다!");
            return;
        }

        // 플레이어 초기 위치와 목표 위치
        Vector3 startPosition = playerTransform.position; // 플레이어 초기 위치
        Vector3 goalPosition = goalObject.transform.position; // 목표 위치

        // 두 점 사이의 거리와 방향 계산
        float totalDistance = Vector3.Distance(startPosition, goalPosition);
        Vector3 direction = (goalPosition - startPosition).normalized;

        // 2m 간격으로 오브젝트 배치
        float interval = 3f; // 간격
        int numObjects = Mathf.FloorToInt(totalDistance / interval); // 생성할 오브젝트 수

        for (int i = 1; i <= numObjects; i++)
        {
            float yOffset = -2;
            // 위치 계산
            Vector3 spawnPosition = startPosition + direction * (i * interval);

            spawnPosition.y += yOffset;

            // 오브젝트 생성
            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log($"오브젝트 생성: {spawnPosition}");
        }
    }


    public GameObject GetClosestGoalObject()
    {
        GameObject closestGoal = null;
        float closestDistance = float.MaxValue;

        // 각 목표 오브젝트와의 거리 계산
        if (goalObject1 != null && goalObject1.activeSelf)
        {
            float distance = Vector3.Distance(playerTransform.position, goalObject1.transform.position);
            if (distance < closestDistance)
            {
                closestGoal = goalObject1;
                closestDistance = distance;
            }
        }

        if (goalObject2 != null && goalObject2.activeSelf)
        {
            float distance = Vector3.Distance(playerTransform.position, goalObject2.transform.position);
            if (distance < closestDistance)
            {
                closestGoal = goalObject2;
                closestDistance = distance;
            }
        }

        if (goalObject3 != null && goalObject3.activeSelf)
        {
            float distance = Vector3.Distance(playerTransform.position, goalObject3.transform.position);
            if (distance < closestDistance)
            {
                closestGoal = goalObject3;
                closestDistance = distance;
            }
        }
        if (goalObject4 != null && goalObject4.activeSelf)
        {
            float distance = Vector3.Distance(playerTransform.position, goalObject4.transform.position);
            if (distance < closestDistance)
            {
                closestGoal = goalObject4;
                closestDistance = distance;
            }
        }
        if (goalObject5 != null && goalObject5.activeSelf)
        {
            float distance = Vector3.Distance(playerTransform.position, goalObject5.transform.position);
            if (distance < closestDistance)
            {
                closestGoal = goalObject5;
                closestDistance = distance;
            }
        }

        return closestGoal;
    }

    /// <summary>
    /// 목표 오브젝트에 도달했을 때 호출되는 메서드.
    /// </summary>
    private void OnNextObjectReached()
    {
        Debug.Log("목표 오브젝트에 도달했습니다!");

        // 게임 성공 처리
        isGameActive = false;
        if (clearUI != null)
        {
            clearUI.SetActive(true);
        }
    }
}
