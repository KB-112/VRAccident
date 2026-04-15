using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public float moveSpeed = 6f;
    public Transform vrCamera;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = vrCamera.forward;
        Vector3 right = vrCamera.right;

     
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * v + right * h;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}