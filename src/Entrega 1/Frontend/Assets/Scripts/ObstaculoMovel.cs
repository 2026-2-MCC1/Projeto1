using UnityEngine;

public class ObstaculoMovel : MonoBehaviour
{
    // Velocidade de movimento do obstáculo
    public float velocidade = 3f;

    // Limites de movimento no eixo X
    public float limiteEsquerda = -3.5f;
    public float limiteDireita = 3.5f;

    // Ligado: os limites contam a partir de onde o obstáculo foi colocado na cena
    public bool limitesRelativos = false;

    // Define a direção inicial do obstáculo
    private bool indoDireita = true;

    // Posição usada como referência quando os limites são relativos
    private float origem = 0f;

    void Start()
    {
        if (limitesRelativos == true)
        {
            origem = transform.position.x;
        }
    }

    void Update()
    {
        // Move o obstáculo para a direita
        if (indoDireita == true)
        {
            transform.position =
                transform.position +
                Vector3.right * velocidade * Time.deltaTime;
        }
        else
        {
            // Move o obstáculo para a esquerda
            transform.position =
                transform.position +
                Vector3.left * velocidade * Time.deltaTime;
        }

        // Ao atingir o limite da direita, muda a direção
        if (transform.position.x >= origem + limiteDireita)
        {
            indoDireita = false;
        }

        // Ao atingir o limite da esquerda, volta para a direita
        if (transform.position.x <= origem + limiteEsquerda)
        {
            indoDireita = true;
        }
    }
}