using UnityEngine;

// BGM・SEの再生と音量管理を担当するシングルトンAudioManager
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;
    public AudioSource seSource;       // 通常の効果音（PlayOneShot 用）
    public AudioSource loopSeSource;   // DriveLoopなどループSE専用

    [Header("SE Clips")]
    public AudioClip commonSE;
    public AudioClip startButtonSE;

    private AudioClip idleLoopClip;
    private AudioClip driveLoopClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ループSEの事前読み込み
        //idleLoopClip = Resources.Load<AudioClip>("SE_IdleLoop");
        driveLoopClip = Resources.Load<AudioClip>("SE_DriveLoop");
    }

    // BGM制御
    public void PlayBGM(AudioClip clip, bool loop = true, float volume = 1.0f)
    {
        if (clip == null || bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = volume * GetVolume("BGMVolume");
        bgmSource.Play();
    }

    // SE制御
    public void PlaySE(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || seSource == null) return;

        seSource.PlayOneShot(clip, volume * GetVolume("SEVolume"));
    }

    public void PlayCommonSE()
    {
        PlaySE(commonSE);
    }

    public void PlayStartButtonSE()
    {
        PlaySE(startButtonSE);
    }

    public void StopSE()
    {
        if (seSource != null)
        {
            seSource.Stop();
            seSource.loop = false;
        }
    }

    public bool IsNotPlaying(string clipName)
    {
        return seSource == null || seSource.clip == null || seSource.clip.name != clipName || !seSource.isPlaying;
    }

    // 任意のSEをループ再生する（例：DriveLoop）
    public void PlayLoopSE(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || loopSeSource == null) return;

        loopSeSource.Stop();
        loopSeSource.clip = clip;
        loopSeSource.loop = true;
        loopSeSource.volume = volume * GetVolume("SEVolume");
        loopSeSource.Play();
    }

    // 加速状態に応じて SE_IdleLoop , SE_DriveLoop を切り替える
    public void HandleDriveLoop(bool isAccelerating)
    {
        if (loopSeSource == null || driveLoopClip == null) return;

        AudioClip targetClip = isAccelerating ? driveLoopClip : null;

        if (targetClip == null)
        {
            if (loopSeSource.isPlaying && loopSeSource.clip == driveLoopClip)
            {
                loopSeSource.Stop();
            }
            return;
        }

        if (loopSeSource.clip != targetClip || !loopSeSource.isPlaying)
        {
            loopSeSource.Stop();
            loopSeSource.clip = targetClip;
            loopSeSource.loop = true;
            loopSeSource.volume = GetVolume("SEVolume");
            loopSeSource.Play();
        }
    }

    public void StopDriveLoop()
    {
        if (loopSeSource != null && loopSeSource.isPlaying)
        {
            loopSeSource.Stop();
            loopSeSource.clip = null;
            loopSeSource.loop = false;
        }
    }

    // 音量取得
    private float GetVolume(string key)
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float type = PlayerPrefs.GetFloat(key, 1f);
        return master * type;
    }

    // BGMとSEの音量をPlayerPrefsの設定値に基づいて毎フレーム反映
    public void ApplyVolumeSettings()
    {
        if (bgmSource != null)
            bgmSource.volume = GetVolume("BGMVolume");

        if (seSource != null)
            seSource.volume = GetVolume("SEVolume");
    }

}
