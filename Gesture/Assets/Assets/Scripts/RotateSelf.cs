using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    public int jointnumber;

    public Vector3 initialRotation; // 用于存储初始旋转角度

    void Start()
    {
        // 获取并存储初始旋转角度
        initialRotation = GetComponent<Transform>().localEulerAngles;
    }

    void Update()
    {
        // 获取当前物体的Transform组件
        Transform myObject = GetComponent<Transform>();


        // 尝试将接收到的字符串转换为浮点数
        if (float.TryParse(UDPManager.latestRecvStr[jointnumber], out float newZAngle))
        {
            // 以初始旋转为基础，创建目标旋转向量
            Vector3 targetRotationVector = initialRotation + new Vector3(0, 0, newZAngle);

            // 检查目标旋转和当前旋转之间的差异是否足够大以应用新旋转（防抖动逻辑）
            if (Vector3.Distance(myObject.localEulerAngles, targetRotationVector) > 0.5f)
            {
                // 使用 Lerp 平滑过渡到新的旋转
                myObject.localEulerAngles = Vector3.Lerp(myObject.localEulerAngles, targetRotationVector, Time.deltaTime * 2.0f);
            }
        }

    }

}
