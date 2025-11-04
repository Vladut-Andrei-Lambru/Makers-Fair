using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Canvas resultCanvas;
    public TextMeshProUGUI resultText;
    
    [Header("Display Settings")]
    public float displayDuration = 3f;
    public Color winColor = Color.green;
    public Color loseColor = Color.red;

    void Start()
    {
        if (PlayerPrefs.HasKey("GameResult"))
        {
            string result = PlayerPrefs.GetString("GameResult");
            ShowResult(result);
            PlayerPrefs.DeleteKey("GameResult");
            PlayerPrefs.Save();
        }
        else
        {
            if (resultCanvas != null)
                resultCanvas.gameObject.SetActive(false);
        }
    }

    void ShowResult(string result)
    {
        if (resultCanvas == null || resultText == null)
        {
            Debug.LogError("[ResultDisplay] Canvas or Text not assigned!");
            return;
        }
        resultCanvas.gameObject.SetActive(true);
        if (result == "WON")
        {
            resultText.text = "YOU WON!The Bridge did not break";
            resultText.color = winColor;
            Debug.Log("[ResultDisplay] Showing WIN screen");
        }
        else if (result == "LOST")
        {
            resultText.text = "YOU LOST! Too heavy for the bridge";
            resultText.color = loseColor;
            Debug.Log("[ResultDisplay] Showing LOSE screen");
        }
        
        Invoke(nameof(HideResult), displayDuration);
    }

    void HideResult()
    {
        if (resultCanvas != null)
        {
            resultCanvas.gameObject.SetActive(false);
            Debug.Log("[ResultDisplay] Result hidden");
        }
    }
}