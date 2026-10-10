using UnityEngine;
using TMPro;

public class MensagemUI : MonoBehaviour
{
    // Texto usado para mostrar mensagens na tela
    public TMP_Text textoMensagem;

    void Start()
    {
        // Inicia o jogo sem nenhuma mensagem aparecendo
        textoMensagem.text = "";
    }

    public void MostrarMensagem(string mensagem)
    {
        // Mostra a mensagem recebida na tela
        textoMensagem.text = mensagem;

        // Cancela qualquer limpeza de mensagem anterior
        CancelInvoke();

        // Limpa a mensagem depois de 2 segundos
        Invoke("LimparMensagem", 2f);
    }

    public void LimparMensagem()
    {
        // Remove a mensagem da tela
        textoMensagem.text = "";
    }
}