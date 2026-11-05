using UnityEngine;

// Vai no Player.
// Guarda o último checkpoint e leva o jogador de volta para ele
// quando ele cai ou encosta em um perigo.
public class RespawnJogador : MonoBehaviour
{
    public Transform pontoInicial;      // onde o jogador volta antes de pegar algum checkpoint
    public float limiteQueda = -9.5f;   // se o jogador ficar abaixo dessa altura, ele caiu

    private Vector3 pontoAtual;         // posição do último checkpoint
    private CharacterController controle;

    void Start()
    {
        controle = GetComponent<CharacterController>();

        // Começa usando o ponto inicial. Se ele não foi colocado, usa a posição atual do jogador.
        if (pontoInicial != null)
        {
            pontoAtual = pontoInicial.position;
        }
        else
        {
            pontoAtual = transform.position;
        }
    }

    void Update()
    {
        if (transform.position.y < limiteQueda)
        {
            VoltarAoCheckpoint();
        }
    }

    // Leva o jogador para o último checkpoint.
    public void VoltarAoCheckpoint()
    {
        // O CharacterController precisa ser desligado para mudar a posição do jogador.
        controle.enabled = false;
        transform.position = pontoAtual;
        controle.enabled = true;
    }

    // Chamado pelo checkpoint quando o jogador passa por ele.
    public void SalvarCheckpoint(Vector3 novoPonto)
    {
        pontoAtual = novoPonto;
    }
}
