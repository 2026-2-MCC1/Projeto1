using UnityEngine;
using TMPro;

public class MensagemUI : MonoBehaviour
{
    public TMP_Text textoMensagem;

    void Start()
    {
        textoMensagem.text = "";
    }

    public void MostrarMensagem(string mensagem)
    {
        textoMensagem.text = mensagem;

        CancelInvoke();
        Invoke("LimparMensagem", 2f);
    }

    public void LimparMensagem()
    {
        textoMensagem.text = "";
    }
}