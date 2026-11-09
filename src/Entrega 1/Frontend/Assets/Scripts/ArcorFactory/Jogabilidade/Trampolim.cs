using System.Collections;
using UnityEngine;

namespace ArcorFactory
{
    // Trampolim de marshmallow: quem cai em cima é lançado bem alto
    public class Trampolim : MonoBehaviour
    {
        public float forca = 13f;
        public Transform parteQueAmassa;

        Collider colisor;
        Vector3 escalaBase;
        float espera;

        void Start()
        {
            colisor = GetComponent<Collider>();
            if (parteQueAmassa == null) parteQueAmassa = transform;
            escalaBase = parteQueAmassa.localScale;
        }

        void Update()
        {
            espera -= Time.deltaTime;
            var ataque = AtaqueJogador.Instancia;
            if (ataque == null || colisor == null || espera > 0f) return;
            if (ataque.PisouEm(colisor.bounds) || (ataque.Corpo.isGrounded && EmCima(ataque)))
            {
                espera = 0.25f;
                ataque.Quicar(forca);
                Sons.TocarEm("trampolim", transform.position, 0.9f, 0.08f);
                Efeitos.Anel(colisor.bounds.center + Vector3.up * colisor.bounds.extents.y, new Color(1f, 0.85f, 0.95f, 0.8f), 3f);
                Efeitos.Brilhos(colisor.bounds.center + Vector3.up * colisor.bounds.extents.y, Cores.Rosa, 10, 0.6f, 3f);
                StopAllCoroutines();
                StartCoroutine(Amassar());
            }
        }

        bool EmCima(AtaqueJogador a)
        {
            Bounds b = colisor.bounds, c = a.Corpo.bounds;
            Vector3 d = b.center - c.center; d.y = 0f;
            return d.magnitude < Mathf.Max(b.extents.x, b.extents.z) && Mathf.Abs(c.min.y - b.max.y) < 0.3f;
        }

        IEnumerator Amassar()
        {
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = t / 0.45f;
                float onda = Mathf.Sin(k * Mathf.PI * 3f) * (1f - k);
                parteQueAmassa.localScale = Vector3.Scale(escalaBase, new Vector3(1f + onda * 0.25f, 1f - onda * 0.4f, 1f + onda * 0.25f));
                yield return null;
            }
            parteQueAmassa.localScale = escalaBase;
        }
    }
}
