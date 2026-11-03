using UnityEngine;

// Trigger colocado logo depois do gerador de árvores (no topo da rampa).
// Quando o jogador passa por aqui, o gerador para de soltar árvores.
[RequireComponent(typeof(BoxCollider))]
public class FimArvores : MonoBehaviour
{
    public GeradorArvores gerador;

    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider outro)
    {
        if (gerador != null && outro.GetComponentInParent<PlayerMovement>() != null)
        {
            gerador.Parar();
        }
    }

    void OnDrawGizmos()
    {
        BoxCollider caixa = GetComponent<BoxCollider>();
        if (caixa == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.2f);
        Gizmos.DrawCube(caixa.center, caixa.size);
        Gizmos.color = new Color(1f, 0.4f, 0f);
        Gizmos.DrawWireCube(caixa.center, caixa.size);
    }
}
