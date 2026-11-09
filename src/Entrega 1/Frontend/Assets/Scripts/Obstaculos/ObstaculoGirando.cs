using UnityEngine;

// Faz o objeto girar sem parar
public class ObstaculoGirando : MonoBehaviour
{
    public Vector3 eixo = new Vector3(0, 1, 0); // em volta de qual eixo ele gira
    public float velocidade = 90f; // graus por segundo

    void Update()
    {
        transform.Rotate(eixo * velocidade * Time.deltaTime);
    }
}
