using UnityEngine;

namespace ArcorFactory
{
    [RequireComponent(typeof(Light))]
    public class LuzPiscante : MonoBehaviour
    {
        public enum Modo { Suave, Falhando, Alerta }
        public Modo modo = Modo.Suave;
        public float velocidade = 2f;
        Light luz;
        float baseI, fase, falha;
        void Start() { luz = GetComponent<Light>(); baseI = luz.intensity; fase = Random.value * 10f; }
        void Update()
        {
            float t = (Time.time + fase) * velocidade;
            switch (modo)
            {
                case Modo.Suave: luz.intensity = baseI * (0.85f + Mathf.Sin(t) * 0.15f); break;
                case Modo.Alerta: luz.intensity = Mathf.Repeat(t, 1f) < 0.5f ? baseI : baseI * 0.1f; break;
                default:
                    falha -= Time.deltaTime;
                    if (falha <= 0f) falha = Random.Range(0.05f, Random.value < 0.1f ? 0.6f : 0.15f);
                    luz.intensity = Random.value < 0.08f ? baseI * 0.15f : baseI * Random.Range(0.85f, 1f);
                    break;
            }
        }
    }
}
