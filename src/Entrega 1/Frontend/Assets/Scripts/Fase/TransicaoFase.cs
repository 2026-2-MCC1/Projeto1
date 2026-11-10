using UnityEngine;
using UnityEngine.SceneManagement;

// Vai em um objeto com Collider marcado como "Is Trigger"
public class TransicaoFase : MonoBehaviour
{
    public string proximaCena = "Fabrica_Parte2";
    public float espera = 1.5f;
    public string mensagem = "Fase concluída! Entrando na linha de produção...";

    private bool usado = false;

    void OnTriggerEnter(Collider outro)
    {
        if (usado == true)
        {
            return;
        }

        // Aceita o jogador desta fase (PlayerMovement) ou das outras (RespawnJogador)
        bool ehJogador = outro.GetComponentInParent<PlayerMovement>() != null
            || outro.GetComponentInParent<RespawnJogador>() != null;
        if (ehJogador == false)
        {
            return;
        }

        usado = true;
        InventarioJogador.CongelarJogador();

        HUDJogo hud = FindFirstObjectByType<HUDJogo>();
        if (hud != null)
        {
            hud.PararTempo();
        }

        MensagemUI mensagemUI = FindFirstObjectByType<MensagemUI>();
        if (mensagemUI != null)
        {
            mensagemUI.MostrarMensagem(mensagem);
        }

        Invoke("MostrarResumo", espera);
    }

    void MostrarResumo()
    {
        ArcorFactory.HUDArcor.MostrarResumoEContinuar(Carregar);
    }

    void Carregar()
    {
        // Usa o fluxo de telas do jogo (Vitória > próxima fase)
        FactoryEscape.GameFlow.SyncLevelWithScene(SceneManager.GetActiveScene().name);
        FactoryEscape.GameFlow.Victory();
    }
}
