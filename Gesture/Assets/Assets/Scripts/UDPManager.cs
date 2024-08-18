using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class UDPManager : MonoBehaviour
{
    public GameObject gesture;
    public GrabCheck grabCheck;
    public Text gestureText;
    public GameObject isChecking;

    public Dropdown gestureDropdown; // ���һ��Dropdown UI Ԫ������ѡ������
    public Button compareButton; // ���һ��Button UI Ԫ�����ڴ����Ƚ�
    public Button uncompareButton;

    public static string[] latestRecvStr;
    private string recvStr;

    private string UDPClientAddRess = "127.0.0.1"; //Ŀ���������ַ
    private int UDPClientPort = 12333; //Ŀ��������˿ں�
    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;
    byte[] recvData = new byte[1024];
    int recvLen = 0;
    Thread connectThread;

    byte[] sendData = new byte[1024];

    private float[][] gestures; // Ԥ���8����������
    private int[] gestureRecognitionCount = new int[8]; // ÿ�����Ƶ�ʶ�����

    private int selectedGestureIndex = 0; // �洢ѡ�����������
    private bool isComparing = false;

    public Text[] differenceTexts; // �洢15������ı�������
    public volatile float[] differenceFloats = new float[15];

    Color orangeColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

    void Start()
    {
        Application.targetFrameRate = 30;

        gestures = new float[8][]
        {
            new float[15] {23.2f,10.9f,27.2f,23.9f,35.7f,79.3f,14.6f,70.4f,66.4f,8.1f,69.4f,80.2f,6.8f,44.0f,100.3f}, //Grab
            new float[15] {30.8f,8.6f,19.1f,100.0f,71.8f,53.3f,102.6f,76.3f,56.7f,100.7f,78.3f,60.5f,92.6f,79.8f,64.9f},//Fist 
            new float[15] {34.7f,17.8f,46.5f,15.6f,5.5f,4.5f,44.8f,116.2f,15.7f,61.2f,115.2f,20.0f,57.6f,121.6f,18.2f},
            new float[15] {32.4f, 31.3f, 27.2f, 12.4f, 2.9f, 5.6f, 15.5f, 1.5f, 7.8f, 42.1f, 118.9f, 17.5f, 46.8f, 114.6f, 14.6f},
            new float[15] {42.0f, 2.7f, 16.6f, 11.3f, 5.1f, 2.9f, 41.5f, 124.2f, 15.5f, 47.3f, 124.8f, 17.3f, 12.1f, 15.8f, 9.6f},
            new float[15] {32.7f, 6.2f, 5.8f, 16.3f, 5.9f, 2.8f, 11.8f, 8.1f, 1.4f, 3.9f, 4.9f, 2.1f, 5.0f, 2.9f, 6.3f},
            new float[15] {39.0f, 6.7f, 6.2f, 94.7f, 59.2f, 11.4f, 109.8f, 52.9f, 11.6f, 109.3f, 52.5f, 14.8f, 22.8f, 14.7f, 13.9f},
            new float[15] {34.4f, 6.9f, 28.0f, 25.8f, 66.5f, 48.7f, 7.7f, 2.2f, 3.8f, 5.3f, 3.2f, 4.9f, 7.5f, 6.7f, 5.2f } // ����8
        };

        // ��ʼ��Dropdown��ѡ��
        gestureDropdown.ClearOptions();
        gestureDropdown.AddOptions(new List<string>(gestureNames));

        // ���Dropdownѡ��仯�ļ�����
        gestureDropdown.onValueChanged.AddListener(OnGestureDropdownValueChanged);

        // ���Button����¼�������
        compareButton.onClick.AddListener(OnCompareButtonClick);
        uncompareButton.onClick.AddListener(OnUncompareButtonClick);

        //InitializeDifferenceText();
    }

    private string[] gestureNames = new string[]
    {
        "Grab", "Fist", "Index", "V Sign", "Spiderman", "Open", "Shaka", "OK"
    };

    public volatile int isChanging = -1;

    void Update()
    {
        if (isComparing) // ������ڽ������ƱȽ�
        {
            isChecking.SetActive(true);

            // ���ƱȽ��߼�
            if (latestRecvStr == null || latestRecvStr.Length != 15)
                return;

            bool isGestureRecognized = false;

            for (int i = 0; i < gestures.Length; i++)
            {
                if (i == selectedGestureIndex) // ����ѡ�������ƽ��бȽ�
                {
                    if (CompareArrays(latestRecvStr, gestures[i], 20f))
                    {
                        Debug.Log(gestureNames[i]);

                        gestureRecognitionCount[i] += 1;
                        Debug.Log(i + "  " + gestureRecognitionCount[i]);

                        if (gestureRecognitionCount[i] >= 1)
                        {
                            Debug.Log("Gesture recognized: " + gestureNames[i]);
                            gestureText.text = "Successfully";
                            gestureText.color = Color.green;
                            ResetGestureRecognitionCount();
                            isGestureRecognized = true;
                            break;
                        }
                    }
                }
            }

            if (!isGestureRecognized)
            {
                Debug.Log("Undetected");
                gestureText.text = "Unknown";
                gestureText.color = Color.red;
                ResetGestureRecognitionCount();
            }
        }
        else
        {
            isChecking.SetActive(false);
        }
    }

    private void OnGestureDropdownValueChanged(int index)
    {
        selectedGestureIndex = index;
    }

    private void OnCompareButtonClick()
    {
        isComparing = true;
    }

    private void OnUncompareButtonClick()
    {
        isComparing = false;
        for (int i = 0; i < differenceTexts.Length ; i++)
        {
            differenceTexts[i].text = "";
        }
    }

    private void ResetGestureRecognitionCount()
    {
        for (int i = 0; i < gestureRecognitionCount.Length; i++)
        {
            gestureRecognitionCount[i] = 0;
        }
    }


    public bool CompareArrays(string[] latestRecvStr, float[] presetArray, float tolerance)
    {
        if (latestRecvStr.Length != presetArray.Length)
            return false;

        int count = latestRecvStr.Length;

        isChanging = 1;

        for (int i = 0; i < latestRecvStr.Length; i++)
        {
            if (float.TryParse(latestRecvStr[i], out float latestValue))
            {
                differenceFloats[i] = -latestValue - presetArray[i];
                differenceTexts[i].text = (-latestValue - presetArray[i]).ToString("F2");
                if (Mathf.Abs(-latestValue - presetArray[i]) > tolerance)
                {
                    differenceTexts[i].color = Color.red;
                    count--;
                }
                else if (Mathf.Abs(-latestValue - presetArray[i])>10f)
                {
                    differenceTexts[i].color = Color.yellow;
                }
                else
                {
                    differenceTexts[i].color = Color.green;
                }
            }
            else
            {
                return false;
            }
        }

        isChanging = 0;

        return count > 12; 
    }

    /// <summary>
    /// ���������������
    /// </summary>
    /// <param name="message"></param>
    public void InitSocket(string message)
    {
        Debug.Log(message);
        UDPClientAddRess = UDPClientAddRess.Trim();
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientAddRess), UDPClientPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12333));//�󶨶˿ںź�IP
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = sender;
        Debug.Log("�ȴ�����");
        SocketSend(message);
        Debug.Log("���ӳɹ�");
        connectThread = new Thread(new ThreadStart(SocketReceve));
        connectThread.Start();
    }
    /// <summary>
    /// �����˷�����Ҫ���͵�����
    /// </summary>
    /// <param name="sendMessage"></param>
    public void SocketSend(string sendMessage)
    {
        //�����������
        sendData = new byte[1024];
        //ת������
        sendData = Encoding.UTF8.GetBytes(sendMessage);
        //�����ݷ��͵������
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }
    /// <summary>
    /// �������Է���˵���Ϣ
    /// </summary>
    void SocketReceve()
    {
        while (true)
        {
            recvData = new byte[1024];
            try
            {
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
            }
            catch (System.Exception e)
            {
                Debug.LogError("�����쳣���쳣��Ϣ��" + e);
            }
            if (recvLen > 0)
            {
                recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);

                latestRecvStr = recvStr.Split(','); // �������½��յ����ַ���

                string arrayContent = string.Join(", ", latestRecvStr);
            }
            else
            {
                Debug.LogError("���ִ���,�������������Ƿ�����");
            }
        }
    }

    private void ResetGestureRecognitionCount(int excludeIndex)
    {
        for (int i = 0; i < gestureRecognitionCount.Length; i++)
        {
            if (i != excludeIndex)
            {
                gestureRecognitionCount[i] = 0;
            }
        }
    }

    /// <summary>
    /// �ر��������������
    /// </summary>
    public void SocketQuit()
    {
        //����̻߳��ھ���Ҫ�ر��߳�
        Debug.Log("����UDP");
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //���ر�socket
        if (socket != null)
        {
            socket.Close();
        }
    }
}
