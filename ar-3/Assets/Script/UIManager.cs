using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text[] targetDistanceTexts; // 각 목표의 거리 텍스트 배열
    public GameObject welcomePopUp;
    public Button welcomeButton;
    public Button secondButton;
    public GameObject secondaryUI;
    public AudioSource audioSource;
    public AudioClip initialClip;
    public AudioClip secondaryClip;
    public Text timerText;
    public Text ClearTime;
    public Text FailTime;

    public GameObject loadOb;

    private float elapsedTime;
    private bool isTimerRunning;


    [Header("UI Elements")]
    public GameObject settingsPanel; // 설정 창
    public GameObject pauseOverlay; // 게임 멈췄을 때 보여줄 오버레이 (선택 사항)


    private bool isGamePaused = false;
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("UIManager: Duplicate instance found, destroying this instance!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (welcomeButton != null)
        {
            welcomeButton.onClick.AddListener(OnFirstTargetReached);
        }
        if (secondButton != null)
        {
            secondButton.onClick.AddListener(OnSecondaryUIButtonClicked);
        }

        // 시작 시 설정 창을 비활성화
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pauseOverlay != null)
        {
          //  pauseOverlay.SetActive(false);
        }

        PlayInitialClip();
    }

    public void UpdateTargetDistance(int targetIndex, double distance)
    {
        if (targetIndex >= 0 && targetIndex < targetDistanceTexts.Length)
        {
            targetDistanceTexts[targetIndex].text = $"Target {targetIndex + 1}: {distance:F1} m";
        }
    }

    public void ShowWelcomePopUp()
    {
        if (welcomePopUp != null)
        {
            welcomePopUp.SetActive(true);
        }
    }

    private void OnWelcomeButtonClicked()
    {
        if (welcomePopUp != null)
        {
            welcomePopUp.SetActive(false);
        }

        if (secondaryUI != null)
        {
            secondaryUI.SetActive(true);
        }

        PlaySecondaryClip();
        StartTimer();
    }

    private void PlayInitialClip()
    {
        if (audioSource != null && initialClip != null)
        {
            audioSource.clip = initialClip;
            audioSource.Play();
        }
    }

    private void PlaySecondaryClip()
    {
        if (audioSource != null && secondaryClip != null)
        {
            audioSource.clip = secondaryClip;
            audioSource.Play();
        }
    }

    private void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
    }

    // 설정 버튼 클릭 시 호출
    public void ToggleSettings()
    {
        if (settingsPanel == null) return;

        isGamePaused = !isGamePaused;

        // 설정 창 활성화/비활성화
        settingsPanel.SetActive(isGamePaused);

        // 게임 상태 조정
        if (isGamePaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    public void OnFirstTargetReached()
    {
        // Welcome 팝업 활성화
        if (welcomePopUp != null)
        {
            welcomePopUp.SetActive(true);
        }

        // Welcome 버튼 클릭 이벤트 연결
        if (welcomeButton != null)
        {
            welcomeButton.onClick.AddListener(() =>
            {
                // Welcome 팝업 비활성화
                if (welcomePopUp != null)
                    welcomePopUp.SetActive(false);

                // Secondary UI와 알 오브젝트 활성화
                if (secondaryUI != null)
                    secondaryUI.SetActive(true);

                ObjectManager.Instance.SpawnObjectAtTarget(); // 알 스폰
                //ObjectManager.Instance.StartBugFollow();      // 벌레 따라오기 시작
                //PlaySecondaryClip();                         // 두 번째 음악 재생
            });
        }
    }

    public void OnSecondaryUIButtonClicked()
    {
        // 알과 UI 비활성화
        if (ObjectManager.Instance.objectToSpawn != null)
        {
            ObjectManager.Instance.objectToSpawn.SetActive(false);
        }
        if (secondaryUI != null)
        {
            secondaryUI.SetActive(false);
        }

        // GPS 좌표를 기반으로 오브젝트를 개별적으로 배치
        for (int i = 0; i < GPSManager2.Instance.lats.Length; i++)
        {
            Vector3 worldPosition = GPSManager2.Instance.GPSLocationToWorldPosition(
                GPSManager2.Instance.lats[i],
                GPSManager2.Instance.longs[i]
            );

            // 각 목표 오브젝트를 GPS 좌표에 따라 생성
            Vector3 goalPosition1 = GPSManager2.Instance.GPSLocationToWorldPosition(
                GPSManager2.Instance.lats[0], GPSManager2.Instance.longs[0]);
            Vector3 goalPosition2 = GPSManager2.Instance.GPSLocationToWorldPosition(
                GPSManager2.Instance.lats[1], GPSManager2.Instance.longs[1]);
            Vector3 goalPosition3 = GPSManager2.Instance.GPSLocationToWorldPosition(
                GPSManager2.Instance.lats[2], GPSManager2.Instance.longs[2]);

            // 목표 오브젝트 생성
            ObjectManager.Instance.SpawnGoalObjects(goalPosition1, goalPosition2, goalPosition3);
        }
        GameObject goalOb = ObjectManager.Instance.GetClosestGoalObject();
        ObjectManager.Instance.SpawnObjectsAlongPath(goalOb, loadOb);
        // 벌레 추적 시작
        ObjectManager.Instance.StartBugFollow();

        // 타이머 시작
        StartTimer();

        // 음악 변경
        if (audioSource != null && secondaryClip != null)
        {
            audioSource.clip = secondaryClip;
            audioSource.Play();
        }
    }

    // 나가기 버튼 클릭 시 호출
    public void ExitSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        ResumeGame(); // 게임 재개
    }

    // 홈 버튼 클릭 시 호출
    public void ReturnToHome()
    {
        ResumeGame(); // 게임 재개
        SceneManager.LoadScene("Game"); // 홈 씬 로드 (씬 이름 변경 가능)
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // 게임 멈춤
        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(true);
        }
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // 게임 재개
        if (pauseOverlay != null)
        {
          //  pauseOverlay.SetActive(false);
        }
    }



    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = $"Time: {elapsedTime:F2} s";
            ClearTime.text = $"Time: {elapsedTime:F2} s";
            FailTime.text = $"Time: {elapsedTime:F2} s";
        }
    }
}