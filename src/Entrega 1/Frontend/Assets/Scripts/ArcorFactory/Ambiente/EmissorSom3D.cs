using System.Collections;
using UnityEngine;

namespace ArcorFactory
{
    public class EmissorSom3D : MonoBehaviour
    {
        public string loop = "maquina_loop";
        public float volume = 0.5f;
        public float alcance = 16f;

        void Start()
        {
            var clip = Sons.Clip(loop);
            if (clip == null) return;
            var f = gameObject.AddComponent<AudioSource>();
            f.clip = clip;
            f.loop = true;
            f.spatialBlend = 1f;
            f.volume = volume;
            f.minDistance = Mathf.Min(3f, alcance * 0.25f);
            f.maxDistance = alcance;
            f.rolloffMode = AudioRolloffMode.Linear;
            f.dopplerLevel = 0f;
            f.time = Random.Range(0f, clip.length);
            f.Play();
        }
    }
}
