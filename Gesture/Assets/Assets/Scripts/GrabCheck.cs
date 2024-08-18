using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GrabCheck : MonoBehaviour
{
    // ����״̬ö��
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

    public State currentState; // ��ǰ״̬

    public Button CheckButton; // UI��ť������

    public void OnButtonClick()
    {
        // ���ݵ�ǰ״ִ̬�в�ͬ�ĺ���
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
        
        StartCoroutine(DelayedAction(0.5f)); // 5�����ʱ
        //Destroy(collision, 1.0f);
        //BrakeBall.GetComponent<Rigidbody>().AddForce(0,-0.1f,0);
    }

    IEnumerator DelayedAction(float delayInSeconds)
    {
        // �ȴ�ָ��������
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

