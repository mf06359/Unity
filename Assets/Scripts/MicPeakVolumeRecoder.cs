using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MicPeakVolumeRecorder : MonoBehaviour
{
    public Button recordButton;
    public Text resultText;

    private AudioClip micClip;
    private const int sampleRate = 44100;
    private const int recordDuration = 1; // 秒

    void Start()
    {
        recordButton.onClick.AddListener(OnRecordButtonPressed);
    }

    void OnRecordButtonPressed()
    {
        if (Microphone.devices.Length == 0)
        {
            resultText.text = "マイクが見つかりません";
            return;
        }

        StartCoroutine(RecordMicAndAnalyze());
    }

    IEnumerator RecordMicAndAnalyze()
    {
        resultText.text = "録音中...";
        micClip = Microphone.Start(null, false, recordDuration, sampleRate);

        // 録音完了まで待つ
        yield return new WaitForSeconds(recordDuration);

        Microphone.End(null);

        float[] samples = new float[sampleRate * recordDuration];
        micClip.GetData(samples, 0);

        float maxVolume = 0f;

        // サンプルの中で最大音量を計算
        foreach (float sample in samples)
        {
            float absSample = Mathf.Abs(sample);
            if (absSample > maxVolume)
                maxVolume = absSample;
        }

        float scaledVolume = Mathf.Clamp01(maxVolume) * 100f;
        resultText.text = $"最大音量: {scaledVolume:F1}";
    }
}
