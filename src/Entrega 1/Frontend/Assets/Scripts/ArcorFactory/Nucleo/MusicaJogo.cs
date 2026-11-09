using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcorFactory
{
    // Trilha sonora: uma música por fase e uma para o menu, trocadas por cena com fade
    public class MusicaJogo : MonoBehaviour
    {
        const float VolumeMusica = 0.32f;
        static MusicaJogo instancia;
        AudioSource fonte;
        string atual = "";
        string desejada = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Iniciar()
        {
            if (instancia != null) return;
            var go = new GameObject("MusicaJogo");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<MusicaJogo>();
            instancia.fonte = go.AddComponent<AudioSource>();
            instancia.fonte.loop = true;
            instancia.fonte.playOnAwake = false;
            instancia.fonte.spatialBlend = 0f;
            instancia.fonte.volume = 0f;
            SceneManager.sceneLoaded += (c, m) => instancia.Escolher(c.name);
            instancia.Escolher(SceneManager.GetActiveScene().name);
        }

        void Escolher(string cena)
        {
            if (cena.Contains("Parte1")) desejada = "musica_fase1";
            else if (cena.Contains("Parte2")) desejada = "musica_fase2";
            else if (cena.Contains("Parte3")) desejada = "musica_fase3";
            else desejada = "musica_menu";
        }

        void Update()
        {
            if (desejada != atual)
            {
                // sai a atual com fade; quando silenciar, troca
                fonte.volume = Mathf.MoveTowards(fonte.volume, 0f, Time.unscaledDeltaTime / 0.8f);
                if (fonte.volume <= 0.001f)
                {
                    var clip = Resources.Load<AudioClip>("ArcorFactory/Musica/" + desejada);
                    atual = desejada;
                    fonte.clip = clip;
                    if (clip != null) fonte.Play(); else fonte.Stop();
                    // a música do menu antiga (FE_MUSIC) é silenciada para não somar
                    var antiga = GameObject.Find("FE_MUSIC");
                    if (antiga != null) { var s = antiga.GetComponent<AudioSource>(); if (s != null) s.Stop(); }
                }
            }
            else if (fonte.clip != null)
                fonte.volume = Mathf.MoveTowards(fonte.volume, VolumeMusica, Time.unscaledDeltaTime / 1.2f);
        }
    }
}
