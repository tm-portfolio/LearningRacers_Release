using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ���ʐݒ�X���C�_�[�ƃ~�j�}�b�vON/OFF�g�O�����ꊇ�Ő��䂷��N���X
// �|�[�Y���j���[��X�^�[�g��ʂ̐ݒ�p�l�����ŋ��ʎg�p�����B
public class VolumeSettingsController : MonoBehaviour
{
    [Header("���ʃX���C�_�[")]
    public Slider masterVolumeSlider;         // �S�̉��ʃX���C�_�[
    public TMP_Text masterVolumeText;         // �S�̉��ʂ̐��l�\���i���j

    public Slider bgmVolumeSlider;            // BGM���ʃX���C�_�[
    public TMP_Text bgmVolumeText;            // BGM���ʂ̐��l�\���i���j

    public Slider seVolumeSlider;             // SE�i���ʉ��j���ʃX���C�_�[
    public TMP_Text seVolumeText;             // SE���ʂ̐��l�\���i���j

    [Header("�~�j�}�b�v�ݒ�")]
    public Toggle minimapToggle;              // �~�j�}�b�vON/OFF�̃g�O��
    public GameObject miniMapUI;              // ���ۂɕ\�������~�j�}�b�v��UI�I�u�W�F�N�g

    void Start()
    {
        LoadInitialSettings();    // PlayerPrefs���珉���ݒ��ǂݍ���
        SetupListeners();         // �eUI���i�ɕύX�C�x���g��o�^
        UpdateAllVolumeTexts();   // �X���C�_�[�ɑΉ����鐔�l�e�L�X�g��\��
    }

    // ���ʂƃ~�j�}�b�v�ݒ�̏����l��PlayerPrefs����ǂݍ���ŃX���C�_�[���ɔ��f
    private void LoadInitialSettings()
    {
        // ���ʃX���C�_�[�̒l��ۑ����ꂽ�ݒ�l����ǂݍ��݁i�Ȃ���΃f�t�H���g�j
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        seVolumeSlider.value = PlayerPrefs.GetFloat("SEVolume", 1f);

        // �~�j�}�b�vON/OFF�g�O���̐ݒ��ǂݍ��ށi1�Ȃ�ON�j
        if (minimapToggle != null)
        {
            minimapToggle.isOn = PlayerPrefs.GetInt("MinimapOn", 1) == 1;

            // �g�O���ɉ����ă~�j�}�b�v�\��ON/OFF��؂�ւ�
            if (miniMapUI != null)
                miniMapUI.SetActive(minimapToggle.isOn);
        }
    }

    // �X���C�_�[��g�O�����ύX���ꂽ�Ƃ��̏�����o�^����
    private void SetupListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        if (minimapToggle != null)
            minimapToggle.onValueChanged.AddListener(OnMinimapToggleChanged);
    }

    // �S�̉��ʃX���C�_�[���ύX���ꂽ�Ƃ��̏���
    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        masterVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();

        // �������f
        AudioListener.volume = value;
    }

    // BGM���ʃX���C�_�[���ύX���ꂽ�Ƃ��̏���
    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();

        // AudioManager�o�R��BGM���ʂ𑦎����f
        AudioManager.Instance?.ApplyVolumeSettings();
    }

    // ���ʉ��iSE�j���ʃX���C�_�[���ύX���ꂽ�Ƃ��̏���
    private void OnSEVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SEVolume", value);
        seVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();
        // SE�̓X���C�_�[�ύX�����ő����f�͂��ꂸ�ASE�Đ����ɓK�p�����
    }

    // �~�j�}�b�v�\��ON/OFF�̃g�O�����ύX���ꂽ�Ƃ��̏���
    public void OnMinimapToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("MinimapOn", isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (miniMapUI != null)
            miniMapUI.SetActive(isOn);
    }

    // ���ʃX���C�_�[�ɉ����Đ��l�e�L�X�g���X�V�i%�\���j
    private void UpdateAllVolumeTexts()
    {
        masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
        bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolumeSlider.value * 100)}%";
        seVolumeText.text = $"{Mathf.RoundToInt(seVolumeSlider.value * 100)}%";
    }
}
