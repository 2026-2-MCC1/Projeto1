using UnityEngine;

// Faz os doces de uma esteira andarem sem parar
public class EsteiraDoces : MonoBehaviour
{
    public Transform carga; // objeto com os doces
    public Vector3 direcao = Vector3.forward; // direção em que a esteira anda (espaço local)
    public float velocidade = 1.2f; // metros por segundo
    public float periodo = 1f; // distância entre dois doces iguais

    private Vector3 posicaoInicial;
    private float deslocamento = 0f;

    void Start()
    {
        if (carga != null)
        {
            posicaoInicial = carga.localPosition;
        }
    }

    void Update()
    {
        if (carga == null || periodo <= 0f)
        {
            return;
        }

        deslocamento = deslocamento + velocidade * Time.deltaTime;
        if (deslocamento >= periodo)
        {
            deslocamento = deslocamento - periodo;
        }

        carga.localPosition = posicaoInicial + direcao.normalized * deslocamento;
    }
}
