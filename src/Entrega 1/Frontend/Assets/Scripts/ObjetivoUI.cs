using UnityEngine;
using TMPro;

public class ObjetivoUI : MonoBehaviour
{
    // Texto usado para mostrar o objetivo atual da fase
    public TMP_Text textoObjetivo;

    // Primeiro objetivo mostrado na fase
    public string objetivoInicial = "Objetivo: Encontre o cartão de acesso";

    void Start()
    {
        // Define o primeiro objetivo quando a fase começa
        textoObjetivo.text = objetivoInicial;
    }

    public void CartaoEncontrado()
    {
        // Atualiza o objetivo depois que o jogador encontra o cartão
        textoObjetivo.text = "Objetivo: Vá até a porta com 12 Rocklets";
    }
}