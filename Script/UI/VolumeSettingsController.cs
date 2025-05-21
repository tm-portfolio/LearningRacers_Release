using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 音量設定スライダーとミニマップON/OFFトグルを一括で制御するクラス
// ポーズメニューやスタート画面の設定パネル内で共通使用される。
public class VolumeSettingsController : MonoBehaviour
{
    [Header("音量スライダー")]
    public Slider masterVolumeSlider;         // 全体音量スライダー
    public TMP_Text masterVolumeText;         // 全体音量の数値表示（％）

    public Slider bgmVolumeSlider;            // BGM音量スライダー
    public TMP_Text bgmVolumeText;            // BGM音量の数値表示（％）

    public Slider seVolumeSlider;             // SE（効果音）音量スライダー
    public TMP_Text seVolumeText;             // SE音量の数値表示（％）

    [Header("ミニマップ設定")]
    public Toggle minimapToggle;              // ミニマップON/OFFのトグル
    public GameObject miniMapUI;              // 実際に表示されるミニマップのUIオブジェクト

    void Start()
    {
        LoadInitialSettings();    // PlayerPrefsから初期設定を読み込む
        SetupListeners();         // 各UI部品に変更イベントを登録
        UpdateAllVolumeTexts();   // スライダーに対応する数値テキストを表示
    }

    // 音量とミニマップ設定の初期値をPlayerPrefsから読み込んでスライダー等に反映
    private void LoadInitialSettings()
    {
        // 音量スライダーの値を保存された設定値から読み込み（なければデフォルト）
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        seVolumeSlider.value = PlayerPrefs.GetFloat("SEVolume", 1f);

        // ミニマップON/OFFトグルの設定を読み込む（1ならON）
        if (minimapToggle != null)
        {
            minimapToggle.isOn = PlayerPrefs.GetInt("MinimapOn", 1) == 1;

            // トグルに応じてミニマップ表示ON/OFFを切り替え
            if (miniMapUI != null)
                miniMapUI.SetActive(minimapToggle.isOn);
        }
    }

    // スライダーやトグルが変更されたときの処理を登録する
    private void SetupListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        seVolumeSlider.onValueChanged.AddListener(OnSEVolumeChanged);

        if (minimapToggle != null)
            minimapToggle.onValueChanged.AddListener(OnMinimapToggleChanged);
    }

    // 全体音量スライダーが変更されたときの処理
    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        masterVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();

        // 即時反映
        AudioListener.volume = value;
    }

    // BGM音量スライダーが変更されたときの処理
    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        bgmVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();

        // AudioManager経由でBGM音量を即時反映
        AudioManager.Instance?.ApplyVolumeSettings();
    }

    // 効果音（SE）音量スライダーが変更されたときの処理
    private void OnSEVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SEVolume", value);
        seVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.Save();
        // SEはスライダー変更だけで即反映はされず、SE再生時に適用される
    }

    // ミニマップ表示ON/OFFのトグルが変更されたときの処理
    public void OnMinimapToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("MinimapOn", isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (miniMapUI != null)
            miniMapUI.SetActive(isOn);
    }

    // 音量スライダーに応じて数値テキストを更新（%表示）
    private void UpdateAllVolumeTexts()
    {
        masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
        bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolumeSlider.value * 100)}%";
        seVolumeText.text = $"{Mathf.RoundToInt(seVolumeSlider.value * 100)}%";
    }
}
