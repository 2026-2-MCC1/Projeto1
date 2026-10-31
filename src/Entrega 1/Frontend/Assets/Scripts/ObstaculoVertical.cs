using UnityEngine;

public class ObstaculoVertical : MonoBehaviour
{
    // Velocidade de movimento do obstáculo
    public float velocidade = 2f;

    // Limites de movimento no eixo Y
    public float limiteBaixo = 0.5f;
    public float limiteAlto = 3f;

    // Ligado: os limites contam a partir de onde o obstáculo foi colocado na cena
    public bool limitesRelativos = false;

    // Define se o obstáculo está subindo
    private bool subindo = true;

    // Posição usada como referência quando os limites são relativos
    private float origem = 0f;

    void Start()
    {
        if (limitesRelativos == true)
        {
            origem = transform.position.y;
        }
    }

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
        if (transform.position.y >= origem + limiteAlto)
        {
            subindo = false;
        }

        // Ao atingir o limite inferior, volta a subir
        if (transform.position.y <= origem + limiteBaixo)
        {
            subindo = true;
        }
    }
}