using UnityEngine;

namespace ArcorFactory
{
    public class Balancar : MonoBehaviour
    {
        public Vector3 eixo = Vector3.forward;
        public float angulo = 6f;
        public float velocidade = 1.2f;
        Quaternion inicio;
        float fase;
        void Start() { inicio = transform.localRotation; fase = Random.value * 10f; }
        void Update() { transform.localRotation = inicio * Quaternion.AngleAxis(Mathf.Sin((Time.time + fase) * velocidade) * angulo, eixo); }
    }
}
