using UnityEngine;

public class StampCollector : MonoBehaviour
{
    public GameObject itemPanel; // 아이템 패널
    public GameObject item1, item2, item3, item4, item5; // 아이템 오브젝트들

    private const string PlayerProgressKey = "PlayerProgress";

    private void Start()
    {
        // 진행 상태 키 초기화 확인
        if (PlayerPrefs.HasKey(PlayerProgressKey))
        {
            Debug.Log($"기존 진행 상태: {PlayerPrefs.GetString(PlayerProgressKey)}");
        }

        // PlayerPrefs 초기화
        PlayerPrefs.DeleteAll(); // 모든 데이터를 삭제
        PlayerPrefs.Save(); // 삭제된 상태 저장

        // PlayerProgressKey 빈 문자열로 초기화
        PlayerPrefs.SetString(PlayerProgressKey, "");
        PlayerPrefs.Save(); // 초기화된 상태 저장

        Debug.Log("Player progress 초기화 완료: " + PlayerPrefs.GetString(PlayerProgressKey));
    }

    public void ActiveCollectStamp()
    {
        if (itemPanel == null) return;

        // 패널 활성화
        itemPanel.SetActive(true);

        // PlayerPrefs에서 저장된 진행 상태 문자열 가져오기
        string storedData = PlayerPrefs.GetString(PlayerProgressKey, "");

        // 각 숫자의 포함 여부 확인
        bool hasItem1 = storedData.Contains("1");
        bool hasItem2 = storedData.Contains("2");
        bool hasItem3 = storedData.Contains("3");
        bool hasItem4 = storedData.Contains("4");

        // 각 아이템 활성화
        if (hasItem1 && item1 != null) item1.SetActive(true);
        if (hasItem2 && item2 != null) item2.SetActive(true);
        if (hasItem3 && item3 != null) item3.SetActive(true);
        if (hasItem4 && item4 != null) item4.SetActive(true);

        // 모든 아이템이 있을 경우 item5 활성화
        if (hasItem1 && hasItem2 && hasItem3 && hasItem4 && item5 != null)
        {
            item5.SetActive(true);
        }
    }


    /// <summary>
    /// 패널 비활성화 버튼 기능
    /// </summary>
    public void ClosePanel()
    {
        if (itemPanel != null)
        {
            itemPanel.SetActive(false);
            Debug.Log("아이템 패널 비활성화");
        }
    }

    /// <summary>
    /// 패널 활성화 버튼 기능
    /// </summary>
    public void OpenPanel()
    {
        if (itemPanel != null)
        {
            itemPanel.SetActive(true);
            Debug.Log("아이템 패널 활성화");
            ActiveCollectStamp();
        }
    }
}

