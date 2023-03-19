using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreTracker : MonoBehaviour
{
    public int PreviousScore;
    public int Highscore;
    public bool firstScore = true;

    public static HighScoreTracker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        GameObject[] objs = GameObject.FindGameObjectsWithTag("HighscoreTracker");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
