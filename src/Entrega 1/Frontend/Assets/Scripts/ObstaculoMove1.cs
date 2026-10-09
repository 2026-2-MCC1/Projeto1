using UnityEngine;

public class ObstaculoMove1 : MonoBehaviour
{
    public float velocidade = 3f;
    public float limiteEsquerda = -3.5f;
    public float limiteDireita = 3.5f;

    private bool indoDireita = true;
    void Start()
    {
        
    }

    void Update()
    {
       if (indoDireita == true)
        {
            transform.position = transform.position + Vector3.right * velocidade * Time.deltaTime;
        }
        else
        {
            transform.position = transform.position + Vector3.left * velocidade * Time.deltaTime;
        }

       if (transform.position.x >= limiteDireita)
       {
            indoDireita = false;
       }

       if (transform.position.x <= limiteEsquerda)
       {
            indoDireita = true;
       }
    }
}
