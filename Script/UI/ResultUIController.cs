using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ���U���g��ʂ�UI�𐧌䂷��N���X�B
// 1��1�̏��s�\���A���ȃx�X�g�̃����L���O�\���A�p�l���؂�ւ���{�^��������S������B
public class ResultUIController : MonoBehaviour
{
    // --- �p�l����UI�v�f ---
    public TextMeshProUGUI resultText;
    public Button resultButton;

    public GameObject resultPanel1; // ���[�X���ʃp�l��
    public GameObject resultPanel2; // ���ȃx�X�g�����L���O�p�l��
    public GameObject resultPanel3; // Thanks�p�l��
    public GameObject panel1;
    public GameObject panel2;
    
    public Button nextButton;
    public Button returnButton;
    public Button backToPanel2Button;
    public Button thankYouButton;

    public GameObject confirmPopupUI;
    public ReturnToStartPopup returnPopup;

    public Image[] rankImages; // �e���ʂɑΉ����郁�_���摜
    public Sprite goldMedal, silverMedal, bronzeMedal;
    public TextMeshProUGUI[] rankTexts;      // ���O�{�^�C���i���ʏ��j
    public TextMeshProUGUI[] bestRankTexts;  // ���ȃx�X�g����
    public TextMeshProUGUI[] bestTimeTexts;  // ���ȃx�X�g�^�C��
    public TextMeshProUGUI[] bestNameTexts;  // ���ȃx�X�g���O
    public TextMeshProUGUI[] testResultTexts; // 1��1�`���̕\���p
    public Image resultImageDisplay; // WIN/LOSE/DRAW�̉摜�\���p

    private void Start()
    {
        AudioManager.Instance?.StopDriveLoop(); // SE�̃��[�v��~

        // �|�b�v�A�b�v�Ȃǂ�UI������
        if (confirmPopupUI != null) confirmPopupUI.SetActive(false);

        // ���U���g�� null �̏ꍇ�͉������Ȃ�
        if (ResultManager.LastRaceResults == null)
        {
            Debug.LogWarning("[ResultUI] LastRaceResults �� null �ł��I");
            return;
        }

        // BGM�Đ�
        if (AudioManager.Instance != null)
        {
            AudioClip resultSceneBGM = Resources.Load<AudioClip>("BGM_ResultScene");
            AudioManager.Instance.PlayBGM(resultSceneBGM);
        }

        // �p�l��������
        resultPanel1.SetActive(true);
        resultPanel2.SetActive(false);
        if (resultPanel3 != null) resultPanel3.SetActive(false);

        var results = ResultManager.LastRaceResults;

        /* �����̎Ԃƃ��[�X���Ɏg�p�\��
        if (results != null)
        {
            Debug.Log($"[ResultUI] ���ʂ���: {results.Count}��");

            // �\���p�摜�̏������i�ő�3�ʂ܂őΉ��j
            results = results.OrderBy(r => r.Position).ToList();

            for (int i = 0; i < results.Count && i < rankTexts.Length; i++)
            {
                var r = results[i];
                if (rankTexts[i] != null)
                {
                    rankTexts[i].text = $"{r.Position}�ʁF{r.Name}�i{r.Time:F2}�b�j";
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
            Debug.LogWarning("[ResultUI] LastRaceResults �� null �ł��I");
            result += "���U���g��񂪌�����܂���ł����B\n";
        }
        */

        // ���s�e�L�X�g�̕\���i1��1�O��j
        string playerName = PlayerPrefs.GetString("PlayerName", "player");
        var sorted = ResultManager.LastRaceResults.OrderBy(r => r.Time).ToList(); // �^�C����

        if (sorted.Count == 2)
        {
            // ���_�����\����
            foreach (var img in rankImages) if (img != null) img.gameObject.SetActive(false);

            resultText.gameObject.SetActive(false);
            foreach (var txt in testResultTexts) if (txt != null) txt.gameObject.SetActive(true);

            // �ォ�珇��1�ʁE2�ʂ�\��
            for (int i = 0; i < 2; i++)
            {
                if (testResultTexts.Length > i)
                {
                    testResultTexts[i].text = $"{i + 1}�ʁF{sorted[i].Name}�i{sorted[i].Time:F2}�b�j";
                }
            }

            // ���s�摜����
            if (sorted[0].Name == playerName)
                ShowResultImage("win");
            else if (sorted[1].Name == playerName)
                ShowResultImage("lose");
            else
                ShowResultImage("draw");
        }
        else
        {
            resultText.text = "���ʂ��擾�ł��܂���ł���";
            resultText.color = Color.black;
        }

        // --- ���ȃx�X�g�����L���O�\������ ---
        string currentPlayer = PlayerPrefs.GetString("PlayerName", "player");
        float currentBest = PlayerPrefs.GetFloat("CurrentTime", -1f);
        int updatedIndex = -1;

        for (int i = 0; i < 5; i++)
        {
            float t = PlayerPrefs.GetFloat($"BestTime_{i}", -1f);
            string n = PlayerPrefs.GetString($"BestTimeName_{i}", "player"); // �� ���O�ۑ�����Ă���Ύg�p

            if (t > 0f && t < 9999f)
            {
                bestRankTexts[i].text = $"{i + 1}��";
                bestTimeTexts[i].text = $"{t:F2}�b";
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

                    // ���ȃx�X�g�X�V���̌��ʉ����Đ�
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

        // ���ȃx�X�g�X�V�e�L�X�g��ǉ��\��
        if (updatedIndex >= 0)
        {
            TextMeshProUGUI updateText = Instantiate(bestNameTexts[updatedIndex], bestNameTexts[updatedIndex].transform.parent);
            updateText.text = "�@���ȃx�X�g�X�V�I";
            updateText.fontSize = 36;
            updateText.fontStyle = FontStyles.Bold;
            updateText.color = new Color(1.0f, 0.4f, 0.0f); // �Z���I�����W

            // �ʒu���E���ɂ��炷
            RectTransform original = bestNameTexts[updatedIndex].rectTransform;
            RectTransform updateRect = updateText.rectTransform;
            updateRect.anchoredPosition = original.anchoredPosition + new Vector2(180f, 0f);
        }

    }

    // ���ʉ摜��SE��\���i"win" / "lose" / "draw"�j
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
        // �e��{�^���ɃN���b�N�C�x���g��o�^

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

        // Panel2�Ŏ��ȃx�X�g�X�VSE��������x��������
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
