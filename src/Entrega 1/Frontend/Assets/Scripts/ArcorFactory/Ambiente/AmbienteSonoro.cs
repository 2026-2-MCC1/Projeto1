using System.Collections;
using UnityEngine;

namespace ArcorFactory
{
    public class AmbienteSonoro : MonoBehaviour
    {
        public static AmbienteSonoro Instancia { get; private set; }

        public string loop = "amb_vale";
        public float volume = 0.45f;
        public string[] sonsSoltos;
        public Vector2 intervaloSoltos = new Vector2(4f, 9f);
        public float volumeSoltos = 0.35f;

        AudioSource a, b;
        bool usandoA = true;
        float proximoSolto;

        void Awake()
        {
            Instancia = this;
            a = CriarFonte();
            b = CriarFonte();
        }

        AudioSource CriarFonte()
        {
            var f = gameObject.AddComponent<AudioSource>();
            f.loop = true;
            f.playOnAwake = false;
            f.spatialBlend = 0f;
            f.volume = 0f;
            return f;
        }

        void Start()
        {
            Trocar(loop, volume, 2f);
            proximoSolto = Time.time + Random.Range(intervaloSoltos.x, intervaloSoltos.y);
        }

        void Update()
        {
            if (sonsSoltos != null && sonsSoltos.Length > 0 && Time.time >= proximoSolto)
            {
                proximoSolto = Time.time + Random.Range(intervaloSoltos.x, intervaloSoltos.y);
                Sons.Tocar(sonsSoltos[Random.Range(0, sonsSoltos.Length)], volumeSoltos * Random.Range(0.6f, 1f), 0.12f);
            }
        }

        public void Trocar(string novoLoop, float novoVolume, float fade = 1.5f)
        {
            var clip = Sons.Clip(novoLoop);
            var atual = usandoA ? a : b;
            if (atual.clip == clip && atual.isPlaying) return;
            var proxima = usandoA ? b : a;
            usandoA = !usandoA;
            proxima.clip = clip;
            proxima.volume = 0f;
            if (clip != null) proxima.Play();
            StopAllCoroutines();
            StartCoroutine(Cruzar(atual, proxima, novoVolume, fade));
        }

        IEnumerator Cruzar(AudioSource sai, AudioSource entra, float alvo, float fade)
        {
            float t = 0f, inicioSai = sai.volume;
            while (t < fade)
            {
                t += Time.deltaTime;
                float k = t / fade;
                sai.volume = Mathf.Lerp(inicioSai, 0f, k);
                entra.volume = Mathf.Lerp(0f, alvo, k);
                yield return null;
            }
            sai.Stop();
            entra.volume = alvo;
        }
    }
}
