using UnityEngine;

public class ResetQueda : MonoBehaviour
{
    public Transform pontoRespawn;
    public float limiteQueda = -5f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        if (transform.position.y < limiteQueda)
        {
            controller.enabled = false;

            transform.position = pontoRespawn.position;

            controller.enabled = true;
        }
    }
}

