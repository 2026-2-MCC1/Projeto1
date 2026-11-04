using UnityEngine;
using System.Collections;

public class InventarioJogador : MonoBehaviour
{
    // Indica se o jogador já encontrou o cartão de acesso
    public bool temCartao = false;

    public RectTransform iconeRocklets;

    public void ColetarVoando(Transform item, float atraso)
    {
        StartCoroutine(VoarParaHud(item, atraso));
    }

    IEnumerator VoarParaHud(Transform item, float atraso)
    {
        yield return new WaitForSeconds(atraso);
        Camera cam = Camera.main;
        Vector3 inicio = item.position;
        Vector3 escala = item.localScale;

        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            item.position = inicio + Vector3.up * Mathf.Sin(t / 0.25f * Mathf.PI * 0.5f) * 1.2f;
            yield return null;
        }

        Vector3 de = item.position;
        t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.5f);
            Vector3 alvo = cam.ScreenToWorldPoint(new Vector3(iconeRocklets.position.x, iconeRocklets.position.y, 2f));
            item.position = Vector3.Lerp(de, alvo, k * k);
            item.localScale = escala * (1f - k * 0.7f);
            yield return null;
        }

        rocklets++;
        Destroy(item.gameObject);
    }


    private int rocklets = 0;
    public int Rocklets { get { return rocklets; } }

    // Vidas (cascos) do jogador
    public int vidasIniciais = 3;
    private int vidas;
    public int Vidas { get { return vidas; } }

    void Awake()
    {
        vidas = vidasIniciais;
    }

    public void PerderVida()
    {
        if (vidas > 0)
        {
            vidas--;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rocklet"))
        {
            rocklets++;
            Destroy(other.gameObject);
        }
    }
}