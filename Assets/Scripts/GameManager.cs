using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI gameOverText;
    private TextMeshProUGUI gateNumberText;

    private int score = 0;
    private bool isGameOver = false;
    private bool gameOverInvoked = false;

    void Start()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in texts)
        {
            string objName = t.gameObject.name;
            if (objName == "ScoreText") scoreText = t;
            else if (objName == "GameOverText") gameOverText = t;
            else if (objName == "GateNumberText") gateNumberText = t;
        }

        UpdateScoreUI();
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        if (gateNumberText != null)
            gateNumberText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    public void AddScore(int gateNum)
    {
        score++;
        UpdateScoreUI();
        ShowGateNumber(gateNum);
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0.3f;
        if (!gameOverInvoked)
        {
            gameOverInvoked = true;
            Invoke("ShowGameOver", 0.5f);
        }
    }

    void ShowGameOver()
    {
        Time.timeScale = 1f;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = score.ToString() + "\n<size=24>press space</size>";
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    void ShowGateNumber(int num)
    {
        if (gateNumberText != null)
        {
            gateNumberText.text = num.ToString();
            gateNumberText.gameObject.SetActive(true);
            CancelInvoke("HideGateNumber");
            Invoke("HideGateNumber", 0.8f);
        }
    }

    void HideGateNumber()
    {
        if (gateNumberText != null)
            gateNumberText.gameObject.SetActive(false);
    }
}