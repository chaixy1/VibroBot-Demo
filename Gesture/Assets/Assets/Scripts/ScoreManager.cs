using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // ����ģʽ

    private int score; // ��������
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            UpdateScoreUI();
        }
    }

    public Text scoreText; // UI Text�������

    void Awake()
    {
        // ʵ�ֵ���ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ��ֹ����ʱ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI(); // ��ʼ��ʱ����UI
    }

    // ����UI Text��ʾ�ķ���
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString();
    }

    // ����ͨ��������������ӷ���
    public void AddScore(int points)
    {
        Score += points; // ���ӷ������Զ�����UI
    }
}
