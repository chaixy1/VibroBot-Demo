using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    public int jointnumber;

    public Vector3 initialRotation; // ���ڴ洢��ʼ��ת�Ƕ�

    void Start()
    {
        // ��ȡ���洢��ʼ��ת�Ƕ�
        initialRotation = GetComponent<Transform>().localEulerAngles;
    }

    void Update()
    {
        // ��ȡ��ǰ�����Transform���
        Transform myObject = GetComponent<Transform>();


        // ���Խ����յ����ַ���ת��Ϊ������
        if (float.TryParse(UDPManager.latestRecvStr[jointnumber], out float newZAngle))
        {
            // �Գ�ʼ��תΪ����������Ŀ����ת����
            Vector3 targetRotationVector = initialRotation + new Vector3(0, 0, newZAngle);

            // ���Ŀ����ת�͵�ǰ��ת֮��Ĳ����Ƿ��㹻����Ӧ������ת���������߼���
            if (Vector3.Distance(myObject.localEulerAngles, targetRotationVector) > 0.5f)
            {
                // ʹ�� Lerp ƽ�����ɵ��µ���ת
                myObject.localEulerAngles = Vector3.Lerp(myObject.localEulerAngles, targetRotationVector, Time.deltaTime * 2.0f);
            }
        }

    }

}
