using UnityEngine;

// BGM�ESE�̍Đ��Ɖ��ʊǗ���S������V���O���g��AudioManager
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;
    public AudioSource seSource;       // �ʏ�̌��ʉ��iPlayOneShot �p�j
    public AudioSource loopSeSource;   // DriveLoop�Ȃǃ��[�vSE��p

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

        // ���[�vSE�̎��O�ǂݍ���
        //idleLoopClip = Resources.Load<AudioClip>("SE_IdleLoop");
        driveLoopClip = Resources.Load<AudioClip>("SE_DriveLoop");
    }

    // BGM����
    public void PlayBGM(AudioClip clip, bool loop = true, float volume = 1.0f)
    {
        if (clip == null || bgmSource == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = volume * GetVolume("BGMVolume");
        bgmSource.Play();
    }

    // SE����
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

    // �C�ӂ�SE�����[�v�Đ�����i��FDriveLoop�j
    public void PlayLoopSE(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null || loopSeSource == null) return;

        loopSeSource.Stop();
        loopSeSource.clip = clip;
        loopSeSource.loop = true;
        loopSeSource.volume = volume * GetVolume("SEVolume");
        loopSeSource.Play();
    }

    // ������Ԃɉ����� SE_IdleLoop , SE_DriveLoop ��؂�ւ���
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

    // ���ʎ擾
    private float GetVolume(string key)
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float type = PlayerPrefs.GetFloat(key, 1f);
        return master * type;
    }

    // BGM��SE�̉��ʂ�PlayerPrefs�̐ݒ�l�Ɋ�Â��Ė��t���[�����f
    public void ApplyVolumeSettings()
    {
        if (bgmSource != null)
            bgmSource.volume = GetVolume("BGMVolume");

        if (seSource != null)
            seSource.volume = GetVolume("SEVolume");
    }

}
