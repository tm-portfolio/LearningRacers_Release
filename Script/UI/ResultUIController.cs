using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// リザルト画面のUIを制御するクラス。
// 1対1の勝敗表示、自己ベストのランキング表示、パネル切り替えやボタン処理を担当する。
public class ResultUIController : MonoBehaviour
{
    // --- パネルやUI要素 ---
    public TextMeshProUGUI resultText;
    public Button resultButton;

    public GameObject resultPanel1; // レース結果パネル
    public GameObject resultPanel2; // 自己ベストランキングパネル
    public GameObject resultPanel3; // Thanksパネル
    public GameObject panel1;
    public GameObject panel2;
    
    public Button nextButton;
    public Button returnButton;
    public Button backToPanel2Button;
    public Button thankYouButton;

    public GameObject confirmPopupUI;
    public ReturnToStartPopup returnPopup;

    public Image[] rankImages; // 各順位に対応するメダル画像
    public Sprite goldMedal, silverMedal, bronzeMedal;
    public TextMeshProUGUI[] rankTexts;      // 名前＋タイム（順位順）
    public TextMeshProUGUI[] bestRankTexts;  // 自己ベスト順位
    public TextMeshProUGUI[] bestTimeTexts;  // 自己ベストタイム
    public TextMeshProUGUI[] bestNameTexts;  // 自己ベスト名前
    public TextMeshProUGUI[] testResultTexts; // 1対1形式の表示用
    public Image resultImageDisplay; // WIN/LOSE/DRAWの画像表示用

    private void Start()
    {
        AudioManager.Instance?.StopDriveLoop(); // SEのループ停止

        // ポップアップなどのUI初期化
        if (confirmPopupUI != null) confirmPopupUI.SetActive(false);

        // リザルトが null の場合は何もしない
        if (ResultManager.LastRaceResults == null)
        {
            Debug.LogWarning("[ResultUI] LastRaceResults が null です！");
            return;
        }

        // BGM再生
        if (AudioManager.Instance != null)
        {
            AudioClip resultSceneBGM = Resources.Load<AudioClip>("BGM_ResultScene");
            AudioManager.Instance.PlayBGM(resultSceneBGM);
        }

        // パネル初期化
        resultPanel1.SetActive(true);
        resultPanel2.SetActive(false);
        if (resultPanel3 != null) resultPanel3.SetActive(false);

        var results = ResultManager.LastRaceResults;

        /* 複数の車とレース時に使用予定
        if (results != null)
        {
            Debug.Log($"[ResultUI] 結果あり: {results.Count}件");

            // 表示用画像の初期化（最大3位まで対応）
            results = results.OrderBy(r => r.Position).ToList();

            for (int i = 0; i < results.Count && i < rankTexts.Length; i++)
            {
                var r = results[i];
                if (rankTexts[i] != null)
                {
                    rankTexts[i].text = $"{r.Position}位：{r.Name}（{r.Time:F2}秒）";
                    rankTexts[i].fontSize = 48;

                    if (r.Name == PlayerPrefs.GetString("PlayerName"))
                    {
                        rankTexts[i].fontStyle = FontStyles.Bold;
                        rankTexts[i].color = new Color(0.81f, 0.16f, 0.16f); // #CF2A2A
                    }
                    else
                    {
                        rankTexts[i].fontStyle = FontStyles.Normal;
                        rankTexts[i].color = Color.black;
                    }

                    rankTexts[i].gameObject.SetActive(true);
                }

                if (rankImages[i] != null)
                {
                    switch (i)
                    {
                        case 0: rankImages[i].sprite = goldMedal; break;
                        case 1: rankImages[i].sprite = silverMedal; break;
                        case 2: rankImages[i].sprite = bronzeMedal; break;
                    }
                    rankImages[i].gameObject.SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogWarning("[ResultUI] LastRaceResults が null です！");
            result += "リザルト情報が見つかりませんでした。\n";
        }
        */

        // 勝敗テキストの表示（1対1前提）
        string playerName = PlayerPrefs.GetString("PlayerName", "player");
        var sorted = ResultManager.LastRaceResults.OrderBy(r => r.Time).ToList(); // タイム順

        if (sorted.Count == 2)
        {
            // メダルを非表示に
            foreach (var img in rankImages) if (img != null) img.gameObject.SetActive(false);

            resultText.gameObject.SetActive(false);
            foreach (var txt in testResultTexts) if (txt != null) txt.gameObject.SetActive(true);

            // 上から順に1位・2位を表示
            for (int i = 0; i < 2; i++)
            {
                if (testResultTexts.Length > i)
                {
                    testResultTexts[i].text = $"{i + 1}位：{sorted[i].Name}（{sorted[i].Time:F2}秒）";
                }
            }

            // 勝敗画像判定
            if (sorted[0].Name == playerName)
                ShowResultImage("win");
            else if (sorted[1].Name == playerName)
                ShowResultImage("lose");
            else
                ShowResultImage("draw");
        }
        else
        {
            resultText.text = "結果が取得できませんでした";
            resultText.color = Color.black;
        }

        // --- 自己ベストランキング表示処理 ---
        string currentPlayer = PlayerPrefs.GetString("PlayerName", "player");
        float currentBest = PlayerPrefs.GetFloat("CurrentTime", -1f);
        int updatedIndex = -1;

        for (int i = 0; i < 5; i++)
        {
            float t = PlayerPrefs.GetFloat($"BestTime_{i}", -1f);
            string n = PlayerPrefs.GetString($"BestTimeName_{i}", "player"); // ← 名前保存されていれば使用

            if (t > 0f && t < 9999f)
            {
                bestRankTexts[i].text = $"{i + 1}位";
                bestTimeTexts[i].text = $"{t:F2}秒";
                bestNameTexts[i].text = n;

                bool isUpdated = (n == currentPlayer && Mathf.Approximately(t, currentBest) && i == 0);

                if (isUpdated)
                {
                    updatedIndex = i;

                    bestRankTexts[i].fontStyle = FontStyles.Bold;
                    bestTimeTexts[i].fontStyle = FontStyles.Bold;
                    bestNameTexts[i].fontStyle = FontStyles.Bold;

                    Color red = new Color(0.81f, 0.16f, 0.16f); // #CF2A2A
                    bestRankTexts[i].color = red;
                    bestTimeTexts[i].color = red;
                    bestNameTexts[i].color = red;

                    // 自己ベスト更新時の効果音を再生
                    AudioClip updateSE = Resources.Load<AudioClip>("SE_BestTimeUpdate");
                    if (updateSE != null)
                    {
                        AudioManager.Instance?.PlaySE(updateSE, 1.0f);
                    }
                }
                else
                {
                    bestRankTexts[i].fontStyle = FontStyles.Normal;
                    bestTimeTexts[i].fontStyle = FontStyles.Normal;
                    bestNameTexts[i].fontStyle = FontStyles.Normal;

                    bestRankTexts[i].color = Color.black;
                    bestTimeTexts[i].color = Color.black;
                    bestNameTexts[i].color = Color.black;
                }

                bestRankTexts[i].gameObject.SetActive(true);
                bestTimeTexts[i].gameObject.SetActive(true);
                bestNameTexts[i].gameObject.SetActive(true);
            }
            else
            {
                bestRankTexts[i]?.gameObject.SetActive(false);
                bestTimeTexts[i]?.gameObject.SetActive(false);
                bestNameTexts[i]?.gameObject.SetActive(false);
            }
        }

        // 自己ベスト更新テキストを追加表示
        if (updatedIndex >= 0)
        {
            TextMeshProUGUI updateText = Instantiate(bestNameTexts[updatedIndex], bestNameTexts[updatedIndex].transform.parent);
            updateText.text = "　自己ベスト更新！";
            updateText.fontSize = 36;
            updateText.fontStyle = FontStyles.Bold;
            updateText.color = new Color(1.0f, 0.4f, 0.0f); // 濃いオレンジ

            // 位置を右横にずらす
            RectTransform original = bestNameTexts[updatedIndex].rectTransform;
            RectTransform updateRect = updateText.rectTransform;
            updateRect.anchoredPosition = original.anchoredPosition + new Vector2(180f, 0f);
        }

    }

    // 結果画像とSEを表示（"win" / "lose" / "draw"）
    public void ShowResultImage(string resultType)
    {
        if (resultImageDisplay == null) return;

        Sprite sprite = null;
        AudioClip clip = null;

        switch (resultType)
        {
            case "win":
                sprite = Resources.Load<Sprite>("ResultImage_Win");
                clip = Resources.Load<AudioClip>("SE_ResultWin");
                break;
            case "lose":
                sprite = Resources.Load<Sprite>("ResultImage_Lose");
                clip = Resources.Load<AudioClip>("SE_ResultLose");
                break;
            case "draw":
                sprite = Resources.Load<Sprite>("ResultImage_Draw");
                clip = Resources.Load<AudioClip>("SE_ResultDraw");
                break;
        }

        if (sprite != null)
        {
            resultImageDisplay.sprite = sprite;
            resultImageDisplay.gameObject.SetActive(true);
        }

        //if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySE(clip, 0.8f);
        }
    }


    private void Awake()
    {
        // 各種ボタンにクリックイベントを登録

        if (nextButton != null && resultPanel1 != null && resultPanel2 != null)
        {
            nextButton.onClick.AddListener(() =>
            {
                resultPanel1.SetActive(false);
                resultPanel2.SetActive(true);
            });
        }

        if (thankYouButton != null)
        {
            thankYouButton.onClick.AddListener(GoToThankYouPanel);
        }


        if (backToPanel2Button != null && resultPanel3 != null && resultPanel2 != null)
        {
            backToPanel2Button.onClick.AddListener(() =>
            {
                resultPanel3.SetActive(false);
                resultPanel2.SetActive(true);
            });
        }

        if (returnButton != null)
        {
            returnButton.onClick.AddListener(OnReturnToStartPressed);
        }

    }

    public void ShowPanel2()
    {
        resultPanel1.SetActive(false);
        resultPanel2.SetActive(true);
        if (resultPanel3 != null) resultPanel3.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCommonSE();
        }

        // Panel2で自己ベスト更新SEをもう一度流す処理
        string currentPlayer = PlayerPrefs.GetString("PlayerName", "player");
        float currentBest = PlayerPrefs.GetFloat("CurrentTime", -1f);

        for (int i = 0; i < 5; i++)
        {
            float t = PlayerPrefs.GetFloat($"BestTime_{i}", -1f);
            string n = PlayerPrefs.GetString($"BestTimeName_{i}", "player");

            bool isUpdated = (n == currentPlayer && Mathf.Approximately(t, currentBest) && i == 0);

            if (isUpdated)
            {
                AudioClip updateSE = Resources.Load<AudioClip>("SE_BestTimeUpdate");
                if (updateSE != null)
                {
                    AudioManager.Instance?.PlaySE(updateSE, 1.0f);
                }
                break;
            }
        }
    }

    public void BackToPanel1()
    {
        panel2.SetActive(false);
        panel1.SetActive(true);

        var results = ResultManager.LastRaceResults;
        string playerName = PlayerPrefs.GetString("PlayerName", "player");

        if (results != null && results.Count == 2)
        {
            var sorted = results.OrderBy(r => r.Time).ToList();

            if (sorted[0].Name == playerName)
                ShowResultImage("win");
            else if (sorted[1].Name == playerName)
                ShowResultImage("lose");
            else
                ShowResultImage("draw");
        }
    }

    private void UnlockAIInfo()
    {
        PlayerPrefs.SetInt("AIInfoUnlocked", 1);
        PlayerPrefs.Save();
    }

    public void GoToThankYouPanel()
    {
        resultPanel2.SetActive(false);
        resultPanel3.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioClip thankYouSE = Resources.Load<AudioClip>("SE_ThankYou");
            AudioManager.Instance.PlaySE(thankYouSE, 1.0f);
        }
    }

    public void OnReturnToStartPressed()
    {
        returnPopup?.ShowPopup();
    }

    public void ConfirmReturnToStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }

    public void CancelReturnToStart()
    {
        if (confirmPopupUI != null)
            confirmPopupUI.SetActive(false);
    }

}
