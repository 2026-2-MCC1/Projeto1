using UnityEngine;

// Vai em um objeto com Collider marcado como "Is Trigger".
// Se o jogador encostar, toca um som e ele volta para o último checkpoint.
public class Perigo : MonoBehaviour
{
    public AudioClip som;        // som que toca quando o jogador encosta
    public float volume = 1f;    // de 0 (mudo) até 1 (volume máximo)

    void OnTriggerEnter(Collider outro)
    {
        RespawnJogador jogador = outro.GetComponent<RespawnJogador>();
        if (jogador != null)
        {
            TocarSom();
            jogador.VoltarAoCheckpoint();
        }
    }

    void TocarSom()
    {
        if (som != null)
        {
            // Toca o som perto da câmera, para ouvir sempre com o mesmo volume.
            AudioSource.PlayClipAtPoint(som, Camera.main.transform.position, volume);
        }
    }
}
