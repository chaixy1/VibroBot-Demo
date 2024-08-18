using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GrabCheck : MonoBehaviour
{
    // 定义状态枚举
    public enum State
    {
        Ungrab,
        Grab,
        Overgrab
    }
    public GameObject Ball;
    public GameObject BrakeBall;
    private Rigidbody Rigidbody;

    public GameObject collision;

    // Start is called before the first frame update
    void Start()
    {
        collision.SetActive(false);
        BrakeBall.SetActive(false);
        Rigidbody = Ball.GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;
        CheckButton.onClick.AddListener(OnButtonClick);
    }

    public State currentState; // 当前状态

    public Button CheckButton; // UI按钮的引用

    public void OnButtonClick()
    {
        // 根据当前状态执行不同的函数
        switch (currentState)
        {
            case State.Ungrab:
                UnGrab();
                break;
            case State.Grab:
                Grab();
                break;
            case State.Overgrab:
                OverGrab();
                break;
        }
    }

    private void UnGrab()
    {
        Rigidbody.useGravity = true;
        Ball.GetComponent<Renderer>().material.color = Color.green;
    }

    private void Grab()
    {
        
    }

    private void OverGrab()
    {
        Ball.GetComponent<Renderer>().material.color = Color.red;
        
        StartCoroutine(DelayedAction(0.5f)); // 5秒的延时
        //Destroy(collision, 1.0f);
        //BrakeBall.GetComponent<Rigidbody>().AddForce(0,-0.1f,0);
    }

    IEnumerator DelayedAction(float delayInSeconds)
    {
        // 等待指定的秒数
        yield return new WaitForSeconds(delayInSeconds);

        Ball.SetActive(false);

        collision.SetActive(true);
        BrakeBall.SetActive(true);
    }

    public void SceneLoad()
    {
        SceneManager.LoadScene("SampleScene");
    }
}

