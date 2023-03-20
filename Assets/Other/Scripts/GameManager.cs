using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Start")]
    [SerializeField] bool enableCountdown = true;
    [SerializeField] int countdown = 3;
    [SerializeField] string countdownEndMessage;
    [SerializeField] AudioClip countdownSound;
    [SerializeField] AudioClip countdownEndSound;
    bool gameStarted = false;

    [Header("Main gameplay")]
    [SerializeField] bool increaseTimer = false;
    [SerializeField] float timeCount = 30f;
    [SerializeField] string timeCountMessage;

    [Header("Game end")]
    [SerializeField] string gameOverMessage;
    [SerializeField] string endGameMessage;
    [SerializeField] string endGameMessageEnd;
    [SerializeField] bool showRemainingTime = false;
    [SerializeField] bool showHighscore = false;
    [SerializeField] string highscoreMessage;

    [Header("Pause")]
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;
    bool isPaused = false;

    [Header("--- UI Objects ---")]
    [SerializeField] GameObject mainGameScreen;
    [SerializeField] GameObject endGameScreen;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI timeCountText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI coinsText;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] TextMeshProUGUI endGameMessageText;
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI healthNum;
    [SerializeField] TextMeshProUGUI bulletsNum;

    int collectedCoins = 0;
    CoinManager[] CoinsInScene;


    public int CollectectCoins { get { return collectedCoins; }  set { collectedCoins = value; } }
    public bool GameStarted { get { return gameStarted; } }

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        CoinsInScene = FindObjectsOfType<CoinManager>(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
        mainGameScreen.SetActive(true);
        endGameScreen.SetActive(false);
        pauseScreen.SetActive(false);
        if (!increaseTimer)
            timerText.text = timeCount.ToString();
        else
        {
            timerText.text = "0";
            timeCount = 0;
        }

        if (enableCountdown)
            StartCoroutine(GameStart());
        else
            gameStarted = true;

        timeCountText.text = timeCountMessage;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isPaused)
                PauseGame();
            else
                ResumeGame();
        }

        if (gameStarted)
        {
            if (increaseTimer)
            {
                timeCount += Time.deltaTime;
                timerText.text = Mathf.RoundToInt(timeCount).ToString();
                if (collectedCoins >= CoinsInScene.Length)
                {
                    StartCoroutine(EndGameScreen());
                }
            }
            else
            {
                if (timeCount > 0)
                {
                    timeCount -= Time.deltaTime;
                }
                else if (timeCount <= 0 && !endGameScreen.activeInHierarchy)
                {
                    StartCoroutine(EndGameScreen());
                    timeCount = 0;
                }
                timerText.text = Mathf.RoundToInt(timeCount).ToString();
            }
        }

        // debug scene change
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (SceneManager.GetActiveScene().buildIndex < 5)

            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                ResumeGame();
            }
            else
                MainMenuButton();
        }
    }

    IEnumerator GameStart()
    {
        for (int i = countdown; i > 0; i--)
        {
            countdown = i;
            countdownText.text = countdown.ToString();
            AudioSource.PlayClipAtPoint(countdownSound, PlayerMovement.Instance.transform.position);
            yield return new WaitForSeconds(1);
        }
        AudioSource.PlayClipAtPoint(countdownEndSound, PlayerMovement.Instance.transform.position);
        countdownText.text = countdownEndMessage;
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
        gameStarted = true;

    }

    public void MainMenuButton()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator EndGameScreen()
    {
        yield return new WaitForSeconds(0.15f);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mainGameScreen.SetActive(false);
        endGameScreen.SetActive(true);
        endGameMessageText.text = endGameMessage + " " + collectedCoins.ToString() + " " + endGameMessageEnd;
        gameOverText.text = gameOverMessage;

        if (!showRemainingTime)
        {
            timerText.gameObject.SetActive(false);
        }

        if (increaseTimer)
            HighScoreTracker.Instance.PreviousScore = Mathf.RoundToInt(timeCount);
        else
            HighScoreTracker.Instance.PreviousScore = collectedCoins;


        if (showHighscore && HighScoreTracker.Instance.PreviousScore > 0)
        {
            if (increaseTimer)
            {
                if (HighScoreTracker.Instance.firstScore)
                {
                    HighScoreTracker.Instance.firstScore = false;
                    HighScoreTracker.Instance.Highscore = 1000;
                }
                if (HighScoreTracker.Instance.PreviousScore < HighScoreTracker.Instance.Highscore)
                {
                    HighScoreTracker.Instance.Highscore = HighScoreTracker.Instance.PreviousScore;
                }
            }
            else
            {
                if (HighScoreTracker.Instance.PreviousScore > HighScoreTracker.Instance.Highscore)
                {
                    HighScoreTracker.Instance.Highscore = HighScoreTracker.Instance.PreviousScore;
                }
            }
            HighScoreTracker.Instance.PreviousScore = 0;
        }
        if (showHighscore)
            highScoreText.text = highscoreMessage + " " + HighScoreTracker.Instance.Highscore;

    }

    void PauseGame()
    {
        isPaused = true;
        pauseScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void UpdateUI()
    {
        coinsText.text = "Coins: " + collectedCoins.ToString();
        healthSlider.maxValue = PlayerMovement.Instance.Health;
        healthSlider.value = PlayerMovement.Instance.CurrHealth;
        healthNum.text = PlayerMovement.Instance.CurrHealth.ToString();
        bulletsNum.text = "Bullets: " + PlayerMovement.Instance.Bullets.ToString();
    }
}
