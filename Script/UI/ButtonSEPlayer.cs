using UnityEngine;
using UnityEngine.UI;

public class ButtonSEPlayer : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSE);
        }
    }

    private void PlayClickSE()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCommonSE();
        }
    }
}
