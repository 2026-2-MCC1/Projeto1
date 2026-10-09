using UnityEngine;

public class ObstaculoVertical : MonoBehaviour
{
    public float velocidade = 2f;
    public float limiteBaixo = 0.5f;
    public float limiteAlto = 3f;

    private bool subindo = true;

    void Start()
    {
        
    }

    void Update()
    {
        if (subindo == true)
        {
            transform.position = transform.position +
                Vector3.up * velocidade * Time.deltaTime;
        }
        else
        {
            transform.position = transform.position +
                Vector3.down * velocidade * Time.deltaTime;
        }

        if (transform.position.y >= limiteAlto)
        {
            subindo = false;
        }

        if (transform.position.y <= limiteBaixo)
        {
            subindo = true;
        }
    }
}


