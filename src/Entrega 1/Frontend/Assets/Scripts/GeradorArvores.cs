using UnityEngine;

// Solta árvores no topo da rampa em horários e posições aleatórias.
// As árvores rolam na direção da seta azul (frente) deste objeto.
public class GeradorArvores : MonoBehaviour
{
    public ArvoreRolante prefabArvore;

    // Tempo aleatório entre uma árvore e outra
    public float intervaloMinimo = 2.2f;
    public float intervaloMaximo = 5f;

    // Quanto a árvore pode nascer para cada lado do centro, e quanto pode desviar rolando
    public float larguraNascer = 2f;
    public float larguraMaxima = 2.5f;

    // Distância que a árvore percorre até se quebrar
    public float percurso = 36f;

    // Velocidade aleatória de cada árvore
    public float velocidadeMinima = 5f;
    public float velocidadeMaxima = 8f;

    // Vira true quando o jogador passa pelo trigger FimArvores (topo da rampa)
    private bool parado = false;
    private float espera = 1f;

    // Chamado pelo FimArvores: o jogador já passou, não precisa mais soltar árvores
    public void Parar()
    {
        parado = true;
        enabled = false;
    }

    void Update()
    {
        espera -= Time.deltaTime;
        if (parado || espera > 0f || prefabArvore == null)
        {
            return;
        }

        espera = Random.Range(intervaloMinimo, intervaloMaximo);

        Vector3 lateral = Vector3.Cross(Vector3.up, transform.forward).normalized;
        float desvio = Random.Range(-larguraNascer, larguraNascer);

        ArvoreRolante arvore = Instantiate(prefabArvore, transform.position + lateral * desvio, Quaternion.identity);
        arvore.Iniciar(
            transform.forward,
            percurso,
            Random.Range(velocidadeMinima, velocidadeMaxima),
            transform.position,
            larguraMaxima
        );
    }
}
