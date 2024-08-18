using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSend : MonoBehaviour
{
    public UDPManager manager;

    public void SendToSever()
    {
        manager.SocketSend("111");
    }

    // Start is called before the first frame update
    void Start()
    {
        manager.InitSocket("");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
