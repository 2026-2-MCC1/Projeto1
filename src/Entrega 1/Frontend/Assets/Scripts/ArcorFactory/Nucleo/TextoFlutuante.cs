using TMPro;
using UnityEngine;

namespace ArcorFactory
{
    // Texto 3D que sobe e some ("+5", "ESCUDO!", "x2"), sempre virado para a câmera
    public class TextoFlutuante : MonoBehaviour
    {
        TextMeshPro texto;
        float vida, duracao;
        Vector3 inicio;
        Color cor;

        public static void Criar(Vector3 pos, string conteudo, Color cor, float tamanho = 4f, float duracao = 1.1f)
        {
            var go = new GameObject("Texto_" + conteudo);
            go.transform.position = pos;
            var tf = go.AddComponent<TextoFlutuante>();
            var tmp = go.AddComponent<TextMeshPro>();
            var fonte = ConfigArcor.Instancia.fonte;
            if (fonte != null) tmp.font = fonte;
            tmp.text = conteudo;
            tmp.fontSize = tamanho;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = cor;
            tmp.outlineWidth = 0.22f;
            tmp.outlineColor = new Color32(10, 20, 45, 255);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.sortingOrder = 50;
            tf.texto = tmp;
            tf.duracao = duracao;
            tf.inicio = pos;
            tf.cor = cor;
        }

        void LateUpdate()
        {
            vida += Time.deltaTime;
            float k = Mathf.Clamp01(vida / duracao);
            transform.position = inicio + Vector3.up * (1.6f * (1f - (1f - k) * (1f - k)));
            float pop = k < 0.15f ? Mathf.Lerp(0.4f, 1.25f, k / 0.15f) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((k - 0.15f) / 0.2f));
            transform.localScale = Vector3.one * pop;
            var cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            Color c = cor;
            c.a = k < 0.7f ? 1f : 1f - (k - 0.7f) / 0.3f;
            texto.color = c;
            if (vida >= duracao) Destroy(gameObject);
        }
    }
}
