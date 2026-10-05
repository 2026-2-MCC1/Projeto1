using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // configuração (aparece no Inspector)
    [SerializeField] private int vidasIniciais = 3;
    [SerializeField] private int minimoProximaFase = 25; // minimo de pontos necessário para conseguir desbloquear a proxima fase


    // estado da partida (só o GameManager mexe)
    private int vidas, pontuacao;
    private bool jogoAcabou;
    private bool proximaFase;
    


    // consulta: outros scripts leem, mas não alteram
    public int Vidas
    {
        get { return vidas; } // para o hud conseguir visualizar 
    }
    public int Pontuacao
    {
        get { return pontuacao; } // para o hud conseguir visualizar 
    }

    void Awake()
    {
        IniciarPartida();
    }

    // um lugar só com os valores iniciais (o botão Reiniciar também usa) 
    private void IniciarPartida()
    {
        vidas = vidasIniciais;
        pontuacao = 0;
        proximaFase = false;
        jogoAcabou = false;
        Debug.Log("Partida iniciada. Vidas: " + vidas);
    }

    // ação: muda o estado e aplica a regra
    public void PerderVida()
    {
        if (jogoAcabou)
        {
            return; // jogo já acabou, ignora
        }

        vidas -= 1;
        Debug.Log("Vidas: " + vidas);

        if (vidas <= 0)
        {
            jogoAcabou = true;
            Debug.Log("Fim de jogo!");
        }
    }

    public void GanharPontos()
    {
        if (jogoAcabou)
        {
            return; // jogo já acabou, ignora
        }

        pontuacao = pontuacao + 1;
        Debug.Log("Pontos: " + pontuacao);

        if (pontuacao >= minimoProximaFase)
        {
            proximaFase = true;
            Debug.Log("Liberado para próxima fase!");
        }
    }




    // teste temporário apenas para validar se está funcionando, quando houver obstaculos, chamar o metodo PerderVida()
    void Update()
    {
        // Keyboard.current é nulo se nenhum teclado estiver conectado
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            PerderVida();
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            GanharPontos();
        }
    }



}