using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;


public class UDPManager : MonoBehaviour
{
    public GameObject gesture;
    public GrabCheck grabCheck;

    private bool setActiveFlag_grab;
    private bool setActiveFlag_fist;
    private bool setActiveFlag_overgrab;

    private int state=10;

    [SerializeField]
    public static string[] latestRecvStr; // 静态变量用于跨脚本通信

    private float[] grab = { 35f, 7.7f, 0.37f, 61.5f, 53.8f, 31.2f, 60f, 61f, 34f, 51.7f, 65.65f, 33.45f, 34.37f, 61.7f, 38.52f };

    public string recvStr; //服务器返回值
    public  string[] latestRecvStr2; // 静态变量用于跨脚本通信

    private string UDPClientAddRess = "127.0.0.1"; //目标服务器地址
    private int UDPClientPort = 12333; //目标服务器端口号
    Socket socket;
    EndPoint serverEnd;
    IPEndPoint ipEnd;
    byte[] recvData = new byte[1024];
    byte[] sendData = new byte[1024];
    int recvLen = 0;
    Thread connectThread;

    private int[] stateFlag=new int[3];

    
    //public Text[] text = new Text[15];

    private float[] textnumber = new float[15];

    public Text gestureText;

    private int[] gestureRecognitionCount = new int[8];

    public volatile Text[] differenceTexts= new Text[15]; // 存储15个差距文本的引用
    public volatile float[] differenceFloats= new float[15];

    public volatile int isChanging=-1;


    void Update()
    {
        if (CompareArrays(latestRecvStr, grab, 25f))
        {
            setActiveFlag_grab = true;
        }
        else
        {
            setActiveFlag_grab = false;

        }

        if (OverGrabCompareArrays(latestRecvStr, grab, 15f))
        {
            setActiveFlag_overgrab = true;
        }
        else
        {
            setActiveFlag_overgrab = false;
        }

        if (setActiveFlag_grab)
        {
            //grabCheck.currentState = GrabCheck.State.Grab;
            if (stateFlag[0] != 0)
            {
                stateFlag[0] += 1;
            }
            else
            {
                stateFlag[0] = 1;
                stateFlag[1] = 0;
                stateFlag[2] = 0;
            }

        }
        else if (setActiveFlag_overgrab)
        {
            //grabCheck.currentState = GrabCheck.State.Overgrab;
            if (stateFlag[1] != 0)
            {
                stateFlag[1] += 1;
            }
            else
            {
                stateFlag[0] = 0;
                stateFlag[1] = 1;
                stateFlag[2] = 0;
            }

        }
        else
        {
            //grabCheck.currentState = GrabCheck.State.Ungrab;
            if (stateFlag[2] != 0)
            {
                stateFlag[2] += 1;
            }
            else
            {
                stateFlag[0] = 0;
                stateFlag[1] = 0;
                stateFlag[2] = 1;
            }
        }


        if (stateFlag[0] >= 5)
        {
            grabCheck.currentState = GrabCheck.State.Grab;
            stateFlag[0] = 0;
        }
        if (stateFlag[1] >= 5)
        {
            grabCheck.currentState = GrabCheck.State.Overgrab;
            stateFlag[1] = 0;
        }
        if (stateFlag[2] >= 5)
        {
            grabCheck.currentState = GrabCheck.State.Ungrab;
            stateFlag[2] = 0;
        }

        Debug.Log(grabCheck.currentState);
    }
    

    /// <summary>
    /// 想服务器发送数据
    /// </summary>
    /// <param name="message"></param>
    public void InitSocket(string message)
    {
        Debug.Log(message);
        UDPClientAddRess = UDPClientAddRess.Trim();
        ipEnd = new IPEndPoint(IPAddress.Parse(UDPClientAddRess), UDPClientPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12333));//绑定端口号和IP
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        serverEnd = sender;
        Debug.Log("等待链接");
        SocketSend(message);
        Debug.Log("连接成功");
        connectThread = new Thread(new ThreadStart(SocketReceve));
        connectThread.Start();
    }
    /// <summary>
    /// 想服务端发送需要发送的内容
    /// </summary>
    /// <param name="sendMessage"></param>
    public void SocketSend(string sendMessage)
    {
        //首先清空所有
        sendData = new byte[1024];
        //转换数据
        sendData = Encoding.UTF8.GetBytes(sendMessage);
        //将数据发送到服务端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
    }
    /// <summary>
    /// 接收来自服务端的消息
    /// </summary>
    void SocketReceve()
    {
        while (true)
        {
            recvData = new byte[1024];
            try
            {
                recvLen = socket.ReceiveFrom(recvData, ref serverEnd);
                //Debug.Log(recvLen);
            }
            catch (System.Exception e)
            {
                Debug.LogError("出现异常，异常信息：" + e);
            }
            //Debug.Log("收到信息,信息来自：" + serverEnd.ToString());
            if (recvLen > 0)
            {
                recvStr = Encoding.UTF8.GetString(recvData, 0, recvLen);
                //Debug.Log(recvStr);
                //ParseData(recvStr);

                latestRecvStr = recvStr.Split(','); // 更新最新接收到的字符串
                latestRecvStr2 = latestRecvStr;
                
                //Debug.Log(CompareArrays(latestRecvStr, rock, 10f));

                string arrayContent = string.Join(", ", latestRecvStr);
                //Debug.Log(arrayContent);

                //if (CompareArrays(latestRecvStr, grab, 25f))
                //{
                //    setActiveFlag_grab = true;
                //    //Debug.Log(setActiveFlag_grab);
                //}
                //else if (CompareArrays(latestRecvStr, fist, 25f))
                //{
                //    setActiveFlag_fist = true;
                //    //Debug.Log(setActiveFlag_grab);
                //}
                //else
                //{
                //    setActiveFlag_grab = false;
                //    setActiveFlag_fist = false;
                //    //Debug.Log(setActiveFlag_grab);
                //}

                //if (OverGrabCompareArrays(latestRecvStr, grab, 10f))
                //{
                //    setActiveFlag_overgrab = true;
                //    //Debug.Log("overgrab");
                //}
                //else
                //{
                //    setActiveFlag_overgrab = false;
                //}
            }
            else
            {
                Debug.LogError("出现错误,请检查网络链接是否正常");
            }
        }
    }

    public bool CompareArrays(string[] latestRecvStr, float[] presetArray, float tolerance)
    {
        // 确保数组长度相同
        if (latestRecvStr.Length != presetArray.Length)
        {
            return false;
        }
        int count = latestRecvStr.Length;

        isChanging = 1;

        for (int i = 0; i < latestRecvStr.Length; i++)
        {
            // 尝试将字符串转换为浮点数
            if (float.TryParse(latestRecvStr[i], out float latestValue))
            {
                differenceFloats[i] = -latestValue - presetArray[i];
                differenceTexts[i].text = (-latestValue - presetArray[i]).ToString("F2");

                if (Mathf.Abs(-latestValue - presetArray[i]) > tolerance)
                {
                    differenceTexts[i].color = Color.yellow;
                    count--;
                }
                else
                {
                    differenceTexts[i].color = Color.green;
                }
            }
            else
            {
                return false; // 转换失败
            }
        }

        isChanging = 0;

        if(count > 12) {
            return true; // 所有元素比较通过
        }
        else
        {
            return false;
        }
        //return true; // 所有元素比较通过
    }

    public bool OverGrabCompareArrays(string[] latestRecvStr, float[] presetArray, float tolerance) 
    {
        // 确保数组长度相同
        if (latestRecvStr.Length != presetArray.Length)
        {
            return false;
        }

        int count = latestRecvStr.Length;
        for (int i = 3; i < latestRecvStr.Length; i++)
        {
            // 尝试将字符串转换为浮点数
            if (float.TryParse(latestRecvStr[i], out float latestValue))
            {
                // 计算差异并与容忍度比较
                if ((-latestValue - presetArray[i]) < tolerance)
                {
                    //textnumber[i] = latestValue - presetArray[i];
                    //return false; 
                    count--;
                }
                else if ((-latestValue - presetArray[i]) > tolerance)
                {
                    differenceTexts[i].color = Color.red;
                }
            }
            else
            {
                return false; // 转换失败
            }
        }

        if (count > 9)
        {
            return true; // 所有元素比较通过
        }
        else
        {
            return false;
        }

        //return true; // 所有元素比较通过
    }

    /// <summary>
    /// 关闭与服务器的连接
    /// </summary>
    public void SocketQuit()
    {
        //如果线程还在就需要关闭线程
        Debug.Log("销毁UDP");
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭socket
        if (socket != null)
        {
            socket.Close();
        }
    }
}
