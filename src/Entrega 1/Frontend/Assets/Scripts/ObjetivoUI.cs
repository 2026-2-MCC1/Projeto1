using UnityEngine;
using TMPro;

public class ObjetivoUI : MonoBehaviour
{
    public TMP_Text textoObjetivo;

    void Start()
    {
        textoObjetivo.text = "Objetivo: Encontre o cartão de acesso";
    }

    public void CartaoEncontrado()
    {
        textoObjetivo.text = "Objetivo: Vá até a porta de saída";
    }
}       