using UnityEngine;
using UnityEngine.Audio;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip riichi;
    [SerializeField] private AudioClip tsumo;

    public void RiichiVoice() { audioSource.PlayOneShot(riichi); }
    public void TsumoVoice() 
    {
        audioSource.PlayOneShot(tsumo);
    }
}
