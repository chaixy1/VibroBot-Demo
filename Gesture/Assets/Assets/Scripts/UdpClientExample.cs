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
    //    int sendPort = 8888; // �������ݵĶ˿�
    //    int receivePort = 8888; // �������ݵĶ˿�

    //    udpClient = new UdpClient(receivePort);
    //    remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort);

    //    // ��ʼ�첽����
    //    udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);

    //    // ����һ����Ϣ
    //    //Send("Hello from Unity!");
    //}

    //// ��������
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

    //// �첽���ջص�
    //private void ReceiveCallback(IAsyncResult ar)
    //{
    //    Debug.Log("111");
    //    // ���udpClient�Ƿ��Ѿ�������
    //    if (udpClient == null)
    //    {
    //       return;
    //    }

    //    try
    //    {
    //        byte[] receiveBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
    //        string receiveString = Encoding.UTF8.GetString(receiveBytes);

    //        Debug.Log("Received: " + receiveString);

    //        // ����������һ����Ϣ
    //        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    //    }
    //    catch (ObjectDisposedException)
    //    {
    //        // UdpClient�ѱ����٣����ټ�����������
    //        Debug.Log("UdpClient has been disposed.");
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(e.ToString());
    //    }
    //}

    //// ����������ʱ�ر�UDP����
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
        int receivePort = 12345; // ���ý������ݵĶ˿�
        udpClient = new UdpClient(receivePort);

        // ����һ���߳�����������
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
                yield break; // ��������쳣���˳�ѭ��
            }

            yield return null; // ȷ��Unity���ᶳ��
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
