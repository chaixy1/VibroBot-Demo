using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class UDPSend : MonoBehaviour
{
    public  UDPManager UDPManager;

    public float[] floatArray; // ����������Ҫ���͵�float����

    UdpClient client = new UdpClient();
    IPEndPoint ip = new IPEndPoint(IPAddress.Parse("192.168.3.2"), 12345);

    void Start()
    {
        Debug.Log("2222");
    }

    private void Update()
    {
        if (UDPManager.isChanging==0) {
            SendData(UDPManager.differenceFloats);
        }
    }


    private void SendData(float[] data)
    {
        // ��float����ת��Ϊ�ֽ�
        byte[] bytes = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        
        client.Send(bytes, bytes.Length, ip);

    }
}
