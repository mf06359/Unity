using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MicPeakVolumeRecorder : MonoBehaviour
{
    public Button recordButton;
    public Text resultText;

    private AudioClip micClip;
    private const int sampleRate = 44100;
    private const int recordDuration = 1; // �b

    void Start()
    {
        recordButton.onClick.AddListener(OnRecordButtonPressed);
    }

    void OnRecordButtonPressed()
    {
        if (Microphone.devices.Length == 0)
        {
            resultText.text = "�}�C�N��������܂���";
            return;
        }

        StartCoroutine(RecordMicAndAnalyze());
    }

    IEnumerator RecordMicAndAnalyze()
    {
        resultText.text = "�^����...";
        micClip = Microphone.Start(null, false, recordDuration, sampleRate);

        // �^�������܂ő҂�
        yield return new WaitForSeconds(recordDuration);

        Microphone.End(null);

        float[] samples = new float[sampleRate * recordDuration];
        micClip.GetData(samples, 0);

        float maxVolume = 0f;

        // �T���v���̒��ōő剹�ʂ��v�Z
        foreach (float sample in samples)
        {
            float absSample = Mathf.Abs(sample);
            if (absSample > maxVolume)
                maxVolume = absSample;
        }

        float scaledVolume = Mathf.Clamp01(maxVolume) * 100f;
        resultText.text = $"�ő剹��: {scaledVolume:F1}";
    }
}
