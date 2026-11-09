using UnityEngine;

// Vai em um objeto com Collider marcado como "Is Trigger"
public class Perigo : MonoBehaviour
{
    public AudioClip som; // som que toca quando o jogador encosta
    public float volume = 1f; // de 0 (mudo) até 1 (volume máximo)

    void Start()
    {
        // Se o som não foi colocado no Inspector, pega o "SomBatida" da pasta Resources
        if (som == null)
        {
            som = Resources.Load<AudioClip>("SomBatida");
        }
    }

    void OnTriggerEnter(Collider outro)
    {
        RespawnJogador jogador = outro.GetComponent<RespawnJogador>();
        if (jogador != null)
        {
            jogador.TocarSom(som, volume);
            jogador.LevarDano();
        }
    }
}
