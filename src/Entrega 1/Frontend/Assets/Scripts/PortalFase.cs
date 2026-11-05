using UnityEngine;
using UnityEngine.SceneManagement;

// Vai em um objeto com Collider marcado como "Is Trigger".
// Quando o jogador entra, carrega a próxima fase.
public class PortalFase : MonoBehaviour
{
    public string proximaCena = ""; // nome da próxima cena (precisa estar em File > Build Profiles)

    void OnTriggerEnter(Collider outro)
    {
        RespawnJogador jogador = outro.GetComponent<RespawnJogador>();
        if (jogador != null)
        {
            if (proximaCena != "")
            {
                SceneManager.LoadScene(proximaCena);
            }
            else
            {
                Debug.Log("Chegou no portal! Escreva o nome da próxima cena no PortalFase.");
            }
        }
    }
}
