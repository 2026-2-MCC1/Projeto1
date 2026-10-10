using UnityEngine;

public class ObstaculoVertical : MonoBehaviour
{
    // Velocidade de movimento do obstáculo
    public float velocidade = 2f;

    // Limites de movimento no eixo Y
    public float limiteBaixo = 0.5f;
    public float limiteAlto = 3f;

    // Define se o obstáculo está subindo
    private bool subindo = true;

    void Update()
    {
        // Move o obstáculo para cima
        if (subindo == true)
        {
            transform.position = transform.position +
                Vector3.up * velocidade * Time.deltaTime;
        }
        else
        {
            // Move o obstáculo para baixo
            transform.position = transform.position +
                Vector3.down * velocidade * Time.deltaTime;
        }

        // Ao atingir o limite superior, começa a descer
        if (transform.position.y >= limiteAlto)
        {
            subindo = false;
        }

        // Ao atingir o limite inferior, volta a subir
        if (transform.position.y <= limiteBaixo)
        {
            subindo = true;
        }
    }
}