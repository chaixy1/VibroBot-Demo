using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab; // 球体Prefab的引用
    public Vector3 spawnPosition; // 球体生成的位置
    public KeyCode spawnKey = KeyCode.Space; // 用于生成球体的按键

    // Update is called once per frame
    void Update()
    {
        // 检测按键是否被按下
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        // 实例化球体Prefab
        GameObject ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);

        // 禁用 Rigidbody 的重力影响
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            StartCoroutine(EnableGravityAfterDelay(rb, 5.0f)); // 5秒后恢复重力
        }
    }

    System.Collections.IEnumerator EnableGravityAfterDelay(Rigidbody rb, float delay)
    {
        // 等待指定时间
        yield return new WaitForSeconds(delay);

        // 重新启用重力
        rb.useGravity = true;
    }
}
