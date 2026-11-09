using System.Collections.Generic;
using UnityEngine;

namespace ArcorFactory
{
    public static class Sons
    {
        const int MaximoFontes = 28;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        static readonly List<AudioSource> fontes = new List<AudioSource>();
        static Transform raiz;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar()
        {
            cache.Clear();
            fontes.Clear();
            raiz = null;
        }

        public static AudioClip Clip(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return null;
            if (!cache.TryGetValue(nome, out AudioClip clip))
            {
                clip = Resources.Load<AudioClip>("ArcorFactory/Sons/" + nome);
                if (clip == null) clip = Resources.Load<AudioClip>(nome); // sons antigos (SomRocklet etc.)
                cache[nome] = clip;
            }
            return clip;
        }

        public static AudioSource Tocar(string nome, float volume = 1f, float variacaoTom = 0.05f)
        {
            return TocarClip(Clip(nome), Vector3.zero, false, volume, variacaoTom, 0f);
        }

        public static AudioSource TocarEm(string nome, Vector3 posicao, float volume = 1f, float variacaoTom = 0.06f, float alcance = 28f)
        {
            return TocarClip(Clip(nome), posicao, true, volume, variacaoTom, alcance);
        }

        public static AudioSource TocarClip(AudioClip clip, Vector3 posicao, bool em3D, float volume, float variacaoTom, float alcance)
        {
            if (clip == null) return null;
            AudioSource f = PegarFonte();
            f.transform.position = posicao;
            f.clip = clip;
            f.volume = Mathf.Clamp01(volume);
            f.pitch = 1f + Random.Range(-variacaoTom, variacaoTom);
            f.spatialBlend = em3D ? 1f : 0f;
            f.minDistance = 2.5f;
            f.maxDistance = Mathf.Max(5f, alcance);
            f.rolloffMode = AudioRolloffMode.Linear;
            f.dopplerLevel = 0f;
            f.loop = false;
            f.Play();
            return f;
        }

        static AudioSource PegarFonte()
        {
            if (raiz == null)
            {
                var go = new GameObject("[ArcorFactory] Sons");
                Object.DontDestroyOnLoad(go);
                raiz = go.transform;
                fontes.Clear();
            }

            AudioSource maisAntiga = null;
            float maiorTempo = -1f;
            for (int i = fontes.Count - 1; i >= 0; i--)
            {
                AudioSource f = fontes[i];
                if (f == null) { fontes.RemoveAt(i); continue; }
                if (!f.isPlaying) return f;
                if (f.time > maiorTempo) { maiorTempo = f.time; maisAntiga = f; }
            }

            if (fontes.Count < MaximoFontes)
            {
                var go = new GameObject("Fonte");
                go.transform.SetParent(raiz, false);
                var nova = go.AddComponent<AudioSource>();
                nova.playOnAwake = false;
                fontes.Add(nova);
                return nova;
            }
            maisAntiga.Stop();
            return maisAntiga;
        }
    }
}
