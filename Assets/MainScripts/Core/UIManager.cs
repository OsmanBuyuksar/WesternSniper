using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject startPanel = null;

    [SerializeField] private GameObject winPanel = null;

    [SerializeField] private GameObject losePanel = null;

    [SerializeField] private GameObject mainPanel = null;

    [SerializeField] private Text levelText = null;

    private void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }

    private void ShowLosePanel()
    {
        losePanel.SetActive(true);
    }

    private void HideStartPanel()
    {
        startPanel.SetActive(false);
    }

    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
    }

    private void HideMainPanel()
    {
        mainPanel.SetActive(false);
    }

    private void UpdateLevelText()
    {
        levelText.text ="LEVEL " + PlayerPrefs.GetInt("LEVELTEXT", 1).ToString();
    }

    private void OnGameStart()
    {
        UpdateLevelText();

        ShowMainPanel();

        HideStartPanel();
    }

    private void OnGameWin()
    {
        ShowWinPanel();

        HideMainPanel();
    }

    private void OnGameLose()
    {
        ShowLosePanel();

        HideMainPanel();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameWin += OnGameWin;
        GameManager.OnGameLose += OnGameLose;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameWin -= OnGameWin;
        GameManager.OnGameLose -= OnGameLose;
    }
}