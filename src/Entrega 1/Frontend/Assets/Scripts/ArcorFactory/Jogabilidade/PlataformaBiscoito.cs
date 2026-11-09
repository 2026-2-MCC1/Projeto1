using System.Collections;
using UnityEngine;

namespace ArcorFactory
{
    // Plataforma de biscoito
    public class PlataformaBiscoito : MonoBehaviour
    {
        public float tempoAteCair = 0.75f;
        public float tempoParaVoltar = 3.5f;
        public Color corFarelo = new Color(0.86f, 0.62f, 0.32f);

        Collider colisor;
        Renderer[] partes;
        Vector3 posBase;
        Quaternion rotBase;
        Vector3 escalaBase;
        bool ativa;

        void Start()
        {
            colisor = GetComponent<Collider>();
            partes = GetComponentsInChildren<Renderer>();
            posBase = transform.position;
            rotBase = transform.rotation;
            escalaBase = transform.localScale;
        }

        void Update()
        {
            if (ativa || colisor == null) return;
            var a = AtaqueJogador.Instancia;
            if (a == null || !a.Corpo.isGrounded) return;
            Bounds b = colisor.bounds, c = a.Corpo.bounds;
            Vector3 d = b.center - c.center; d.y = 0f;
            bool emCima = Mathf.Abs(d.x) < b.extents.x + 0.2f && Mathf.Abs(d.z) < b.extents.z + 0.2f && Mathf.Abs(c.min.y - b.max.y) < 0.35f;
            if (emCima) StartCoroutine(Cair());
        }

        IEnumerator Cair()
        {
            ativa = true;
            Sons.TocarEm("biscoito_rachar", transform.position, 0.8f);
            float t = 0f;
            while (t < tempoAteCair)
            {
                t += Time.deltaTime;
                float f = Mathf.Lerp(0.02f, 0.07f, t / tempoAteCair);
                transform.position = posBase + new Vector3(Random.Range(-f, f), 0f, Random.Range(-f, f));
                yield return null;
            }
            Efeitos.Farelos(colisor.bounds.center, corFarelo, 22);
            Sons.TocarEm("biscoito_quebrar", transform.position, 0.9f);
            colisor.enabled = false;

            float v = 0f;
            t = 0f;
            Vector3 eixo = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                v += 18f * Time.deltaTime;
                transform.position += Vector3.down * v * Time.deltaTime;
                transform.Rotate(eixo, 60f * Time.deltaTime, Space.World);
                transform.localScale = escalaBase * Mathf.Lerp(1f, 0.6f, t / 1.2f);
                yield return null;
            }
            foreach (var r in partes) r.enabled = false;
            yield return new WaitForSeconds(tempoParaVoltar);

            transform.position = posBase;
            transform.rotation = rotBase;
            foreach (var r in partes) r.enabled = true;
            t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                transform.localScale = escalaBase * Mathf.SmoothStep(0.2f, 1f, t / 0.35f);
                yield return null;
            }
            transform.localScale = escalaBase;
            colisor.enabled = true;
            Efeitos.Brilhos(colisor.bounds.center, Cores.Creme, 8);
            ativa = false;
        }
    }
}
