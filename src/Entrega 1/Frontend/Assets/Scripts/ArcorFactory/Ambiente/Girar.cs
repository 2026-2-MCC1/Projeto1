using UnityEngine;

namespace ArcorFactory
{
    public class Girar : MonoBehaviour
    {
        public Vector3 eixo = Vector3.up;
        public float grausPorSegundo = 90f;
        void Update() { transform.Rotate(eixo, grausPorSegundo * Time.deltaTime, Space.Self); }
    }
}
