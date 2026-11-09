using UnityEngine;

namespace ArcorFactory
{
    // Sombra redonda embaixo da tartaruga (como nos jogos de plataforma 3D)
    public class SombraJogador : MonoBehaviour
    {
        public float tamanho = 1.1f;
        public float alturaMaxima = 14f;

        Transform disco;
        Renderer render;
        CharacterController corpo;
        MaterialPropertyBlock bloco;

        void Start()
        {
            corpo = GetComponent<CharacterController>();
            var mat = ConfigArcor.Instancia.sombraJogador;
            if (mat == null) { enabled = false; return; }

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Sombra_Jogador";
            Destroy(go.GetComponent<Collider>());
            disco = go.transform;
            render = go.GetComponent<Renderer>();
            render.sharedMaterial = mat;
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            render.receiveShadows = false;
            bloco = new MaterialPropertyBlock();
        }

        void LateUpdate()
        {
            if (disco == null) return;
            Vector3 origem = transform.position + Vector3.up * 0.2f;
            if (corpo != null) origem = new Vector3(corpo.bounds.center.x, corpo.bounds.min.y + 0.15f, corpo.bounds.center.z);

            if (Physics.Raycast(origem, Vector3.down, out RaycastHit hit, alturaMaxima, ~0, QueryTriggerInteraction.Ignore))
            {
                render.enabled = true;
                float distancia = hit.distance;
                float k = Mathf.Clamp01(distancia / 6f);
                disco.position = hit.point + hit.normal * 0.03f;
                disco.rotation = Quaternion.LookRotation(-hit.normal) ;
                disco.localScale = Vector3.one * tamanho * Mathf.Lerp(1f, 0.55f, k);
                bloco.SetColor("_BaseColor", new Color(0f, 0f, 0f, Mathf.Lerp(0.55f, 0.18f, k)));
                render.SetPropertyBlock(bloco);
            }
            else
            {
                render.enabled = false;
            }
        }

        void OnDestroy()
        {
            if (disco != null) Destroy(disco.gameObject);
        }
    }
}
