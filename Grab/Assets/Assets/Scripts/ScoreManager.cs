using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // 单例模式

    private int score; // 分数变量
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            UpdateScoreUI();
        }
    }

    public Text scoreText; // UI Text组件引用

    void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 防止重载时销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI(); // 初始化时更新UI
    }

    // 更新UI Text显示的分数
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString();
    }

    // 可以通过这个方法来增加分数
    public void AddScore(int points)
    {
        Score += points; // 增加分数并自动更新UI
    }
}
