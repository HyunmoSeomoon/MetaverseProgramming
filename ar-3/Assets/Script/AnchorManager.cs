using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils; // XR Origin 관련 네임스페이스 추가


public class AnchorManager : MonoBehaviour
{
    public static AnchorManager Instance;

    [Header("AR 설정")]
    public ARAnchorManager anchorManager;
    public XROrigin sessionOrigin;
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
    /// GPS 데이터를 기반으로 AR Anchor를 생성.
    /// </summary>
    /// <param name="position">생성할 앵커의 월드 위치</param>
    /// <param name="rotation">생성할 앵커의 월드 회전</param>
    /// <returns>생성된 ARAnchor</returns>
    public ARAnchor CreateAnchor(Vector3 position, Quaternion rotation)
    {
        if (anchorManager == null)
        {
            Debug.LogError("ARAnchorManager가 설정되지 않았습니다.");
            return null;
        }

        // 앵커를 생성할 게임 오브젝트 생성
        GameObject anchorObject = new GameObject("ARAnchor");
        anchorObject.transform.position = position;
        anchorObject.transform.rotation = rotation;

        // ARAnchor 컴포넌트 추가
        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        // 앵커 생성 확인
        if (anchor == null)
        {
            Debug.LogError("ARAnchor 생성에 실패했습니다.");
            Destroy(anchorObject);
            return null;
        }

        Debug.Log($"ARAnchor 생성 완료: 위치({position}), 회전({rotation})");
        return anchor;
    }

    /// <summary>
    /// 기존 앵커를 제거.
    /// </summary>
    /// <param name="anchor">제거할 ARAnchor</param>
    public void RemoveAnchor(ARAnchor anchor)
    {
        if (anchorManager == null || anchor == null)
        {
            Debug.LogError("ARAnchorManager 또는 ARAnchor가 null입니다.");
            return;
        }

        if (anchorManager.RemoveAnchor(anchor))
        {
            Debug.Log("ARAnchor 제거 완료.");
        }
        else
        {
            Debug.LogWarning("ARAnchor 제거 실패.");
        }
    }
}
