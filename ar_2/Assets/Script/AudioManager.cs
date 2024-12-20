using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer; // AudioMixer 연결
    public Slider volumeSlider;   // 볼륨 조절 슬라이더

    private void Start()
    {
        // 슬라이더 값 초기화 (AudioMixer의 볼륨 값을 가져옴)
        float currentVolume;
        audioMixer.GetFloat("Volume", out currentVolume);
        volumeSlider.value = Mathf.Pow(10, currentVolume / 20); // Linear 값으로 변환

        // 슬라이더 변경 이벤트 연결
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    /// <summary>
    /// 슬라이더 값 변경 시 호출
    /// </summary>
    /// <param name="value">슬라이더 값</param>
    public void SetVolume(float value)
    {
        // 슬라이더 값을 Logarithmic Scale로 변환 후 AudioMixer에 설정
        float volume = Mathf.Log10(value) * 20;
        audioMixer.SetFloat("Volume", volume);
        Debug.Log($"볼륨 조정: {volume} dB");
    }
}
