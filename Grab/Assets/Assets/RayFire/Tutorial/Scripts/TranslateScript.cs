using UnityEngine;

public class TranslateScript : MonoBehaviour
{
    public enum MoveType
    {
        MovePosition = 0,
        Translate = 1
    }
    
    public MoveType moveType;
    public float    speed = 5f;
    Rigidbody       rbody;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (moveType == MoveType.MovePosition) 
            rbody.MovePosition(transform.position + Vector3.right * Time.deltaTime * speed);
    }

    // Update is called once per frame
    void Update()
    {
        if (moveType == MoveType.Translate) 
            transform.Translate(Vector3.right * speed * 0.01f);
    }
}