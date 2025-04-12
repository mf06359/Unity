using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MicPeakVolumeRecorder : MonoBehaviour
{
    public Button recordButton;
    public Text resultText;

    private AudioClip micClip;
    private const int sampleRate = 44100;
    private const int recordDuration = 1; // ïb

    void Start()
    {
        recordButton.onClick.AddListener(OnRecordButtonPressed);
    }

    void OnRecordButtonPressed()
    {
        if (Microphone.devices.Length == 0)
        {
            resultText.text = "É}ÉCÉNÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ";
            return;
        }

        StartCoroutine(RecordMicAndAnalyze());
    }

    IEnumerator RecordMicAndAnalyze()
    {
        resultText.text = "ò^âπíÜ...";
        micClip = Microphone.Start(null, false, recordDuration, sampleRate);

        // ò^âπäÆóπÇ‹Ç≈ë“Ç¬
        yield return new WaitForSeconds(recordDuration);

        Microphone.End(null);

        float[] samples = new float[sampleRate * recordDuration];
        micClip.GetData(samples, 0);

        float maxVolume = 0f;

        // ÉTÉìÉvÉãÇÃíÜÇ≈ç≈ëÂâπó ÇåvéZ
        foreach (float sample in samples)
        {
            float absSample = Mathf.Abs(sample);
            if (absSample > maxVolume)
                maxVolume = absSample;
        }

        float scaledVolume = Mathf.Clamp01(maxVolume) * 100f;
        resultText.text = $"ç≈ëÂâπó : {scaledVolume:F1}";
    }
}
