using UnityEngine;

// Vai em um objeto com BoxCollider marcado como "Is Trigger".
// Quando o jogador passa por aqui, este vira o novo ponto de respawn.
public class CheckpointFase : MonoBehaviour
{
    public float alturaRespawn = 1.25f; // altura acima do chão em que o jogador reaparece
    public Renderer marcador;           // o disco no chão que muda de cor
    public Material materialAtivo;      // cor do disco depois de ativado

    private bool ativado = false;

    void OnTriggerEnter(Collider outro)
    {
        // Só funciona uma vez.
        if (ativado == true)
        {
            return;
        }

        // Confere se quem entrou é o jogador.
        RespawnJogador jogador = outro.GetComponent<RespawnJogador>();
        if (jogador != null)
        {
            Vector3 ponto = transform.position;
            ponto.y = ponto.y + alturaRespawn;
            jogador.SalvarCheckpoint(ponto);

            ativado = true;
            TrocarCor();
        }
    }

    void TrocarCor()
    {
        if (marcador != null && materialAtivo != null)
        {
            marcador.material = materialAtivo;
        }
    }
}
