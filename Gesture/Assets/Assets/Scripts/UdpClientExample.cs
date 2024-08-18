using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpClientExample : MonoBehaviour
{
    //private UdpClient udpClient;
    //private IPEndPoint remoteEndPoint;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    int sendPort = 8888; // 发送数据的端口
    //    int receivePort = 8888; // 接收数据的端口

    //    udpClient = new UdpClient(receivePort);
    //    remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

    //    // 开始异步接收
    //    udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);

    //    // 发送一条消息
    //    //Send("Hello from Unity!");
    //}

    //// 发送数据
    //private void Send(string message)
    //{
    //    try
    //    {
    //        byte[] data = Encoding.UTF8.GetBytes(message);
    //        udpClient.Send(data, data.Length, remoteEndPoint);
    //    }
    //    catch (Exception err)
    //    {
    //        Debug.LogError(err.ToString());
    //    }
    //}

    //// 异步接收回调
    //private void ReceiveCallback(IAsyncResult ar)
    //{
    //    Debug.Log("111");
    //    // 检查udpClient是否已经被销毁
    //    if (udpClient == null)
    //    {
    //       return;
    //    }

    //    try
    //    {
    //        byte[] receiveBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
    //        string receiveString = Encoding.UTF8.GetString(receiveBytes);

    //        Debug.Log("Received: " + receiveString);

    //        // 继续监听下一条消息
    //        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    //    }
    //    catch (ObjectDisposedException)
    //    {
    //        // UdpClient已被销毁，不再继续接收数据
    //        Debug.Log("UdpClient has been disposed.");
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(e.ToString());
    //    }
    //}

    //// 当对象销毁时关闭UDP连接
    //private void OnDestroy()
    //{
    //    if (udpClient != null)
    //    {
    //        udpClient.Close();
    //    }
    //}
    private UdpClient udpClient;

    void Start()
    {
        int receivePort = 12345; // 设置接收数据的端口
        udpClient = new UdpClient(receivePort);

        // 启动一个线程来接收数据
        StartCoroutine(ReceiveData());
    }

    private IEnumerator ReceiveData()
    {
        while (true)
        {
            try
            {
                Debug.Log("11");
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                string receiveString = Encoding.UTF8.GetString(receiveBytes);

                Debug.Log("Received: " + receiveString);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                yield break; // 如果发生异常，退出循环
            }

            yield return null; // 确保Unity不会冻结
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }
}
