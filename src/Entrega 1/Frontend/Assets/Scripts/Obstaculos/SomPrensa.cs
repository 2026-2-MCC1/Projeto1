using UnityEngine;

// Vai na prensa (junto com o ObstaculoVertical ou a PrensaIndustrial)
[RequireComponent(typeof(AudioSource))]
public class SomPrensa : MonoBehaviour
{
    public AudioClip somBatida; // som do impacto
    public float volume = 0.9f;
    public float variacaoTom = 0.06f;

    // Desligado: quem chama Tocar() é outro script
    public bool automatico = true;

    private AudioSource fonte;
    private float alturaAnterior;
    private bool descendo = false;

    void Awake()
    {
        // As prensas da fase 1 usam o ObstaculoVertical (sobe e desce suave)
        ObstaculoVertical antigo = GetComponent<ObstaculoVertical>();
        if (antigo != null && GetComponent<PrensaIndustrial>() == null)
        {
            PrensaIndustrial prensa = gameObject.AddComponent<PrensaIndustrial>();
            prensa.limiteBaixo = antigo.limiteBaixo;
            prensa.limiteAlto = antigo.limiteAlto;
            prensa.limitesRelativos = antigo.limitesRelativos;

            // Cada prensa começa num tempo diferente, para não baterem todas juntas
            prensa.atrasoInicial = Mathf.Repeat((transform.position.x + 20f) * 0.45f, 2f);

            antigo.enabled = false;
        }
    }

    void Start()
    {
        fonte = GetComponent<AudioSource>();
        alturaAnterior = transform.position.y;
    }

    void Update()
    {
        if (automatico == false)
        {
            return;
        }

        float altura = transform.position.y;
        float diferenca = altura - alturaAnterior;

        if (diferenca < -0.0001f)
        {
            descendo = true;
        }
        else if (diferenca > 0.0001f)
        {
            // Estava descendo e começou a subir: acabou de bater embaixo
            if (descendo == true)
            {
                Tocar();
            }
            descendo = false;
        }

        alturaAnterior = altura;
    }

    public void Tocar()
    {
        if (somBatida == null)
        {
            return;
        }

        fonte.pitch = 1f + Random.Range(-variacaoTom, variacaoTom);
        fonte.PlayOneShot(somBatida, volume);
    }
}
