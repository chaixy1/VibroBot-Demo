using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab; // ����Prefab������
    public Vector3 spawnPosition; // �������ɵ�λ��
    public KeyCode spawnKey = KeyCode.Space; // ������������İ���

    // Update is called once per frame
    void Update()
    {
        // ��ⰴ���Ƿ񱻰���
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        // ʵ��������Prefab
        GameObject ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);

        // ���� Rigidbody ������Ӱ��
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            StartCoroutine(EnableGravityAfterDelay(rb, 5.0f)); // 5���ָ�����
        }
    }

    System.Collections.IEnumerator EnableGravityAfterDelay(Rigidbody rb, float delay)
    {
        // �ȴ�ָ��ʱ��
        yield return new WaitForSeconds(delay);

        // ������������
        rb.useGravity = true;
    }
}
