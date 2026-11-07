using UnityEngine;

// Vai em um objeto com BoxCollider marcado como "Is Trigger".
// Quando o jogador passa por aqui, este vira o novo ponto de respawn.
public class CheckpointFase : MonoBehaviour
{
    public float alturaRespawn = 1.25f; // altura acima do chão em que o jogador reaparece
    public Renderer marcador;           // o disco no chão que muda de cor
    public Material materialAtivo;      // cor do disco depois de ativado
    public AudioClip som;               // som que toca quando o checkpoint é ativado
    public float volume = 0.8f;

    private bool ativado = false;

    void Start()
    {
        // Se o som não foi colocado no Inspector, pega o "SomCheckpoint" da pasta Resources.
        if (som == null)
        {
            som = Resources.Load<AudioClip>("SomCheckpoint");
        }
    }

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
            jogador.TocarSom(som, volume);
            jogador.MostrarMensagem("Checkpoint salvo!");
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
