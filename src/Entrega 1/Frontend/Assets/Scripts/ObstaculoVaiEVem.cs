using UnityEngine;

// Faz o objeto ir e voltar em linha reta.
// Exemplo: direcao (1, 0, 0) anda para os lados; direcao (0, 1, 0) sobe e desce.
public class ObstaculoVaiEVem : MonoBehaviour
{
    public Vector3 direcao = new Vector3(1, 0, 0); // para onde ele anda
    public float distancia = 2.8f;   // quanto ele anda para cada lado a partir do ponto inicial
    public float velocidade = 3f;    // metros por segundo
    public float comecarEm = 0f;     // posição inicial entre -distancia e +distancia

    private Vector3 pontoInicial;
    private float deslocamento;      // quanto ele já andou a partir do ponto inicial
    private int sentido = 1;         // 1 = indo, -1 = voltando

    void Start()
    {
        pontoInicial = transform.position;
        deslocamento = comecarEm;
    }

    void Update()
    {
        deslocamento = deslocamento + velocidade * sentido * Time.deltaTime;

        // Chegou no fim de um lado: inverte o sentido.
        if (deslocamento > distancia)
        {
            deslocamento = distancia;
            sentido = -1;
        }
        else if (deslocamento < -distancia)
        {
            deslocamento = -distancia;
            sentido = 1;
        }

        transform.position = pontoInicial + direcao * deslocamento;
    }
}