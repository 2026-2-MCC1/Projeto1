using UnityEngine;
using UnityEngine.SceneManagement;

// Vai em um objeto com Collider marcado como "Is Trigger".
// O portal começa desligado. Ele liga quando o jogador junta a meta de Rocklets.
// Ligado: o jogador entra, o tempo para e a próxima fase é carregada.
public class PortalFase : MonoBehaviour
{
    public string proximaCena = "";     // nome da próxima cena (precisa estar em File > Build Profiles)
    public int metaRocklets = 25;       // quantos Rocklets precisa para abrir

    public RespawnJogador jogador;      // o Player
    public HUDJogo hud;                 // para parar o relógio no final

    public Renderer[] partesPortal;     // partes que mudam de cor quando o portal liga
    public Material materialDesligado;
    public Material materialLigado;
    public AudioClip somAbrir;          // som quando o portal liga

    private InventarioJogador inventario;
    private bool aberto = false;
    private bool usado = false;

    void Start()
    {
        // Se não foram colocados no Inspector, procura na cena.
        if (jogador == null)
        {
            jogador = FindFirstObjectByType<RespawnJogador>();
        }
        if (hud == null)
        {
            hud = FindFirstObjectByType<HUDJogo>();
        }

        // Se o som ou o material não foram colocados no Inspector, pega da pasta Resources.
        if (somAbrir == null)
        {
            somAbrir = Resources.Load<AudioClip>("SomPortal");
        }
        if (materialDesligado == null)
        {
            materialDesligado = Resources.Load<Material>("Fab_PortalDesligado");
        }

        inventario = jogador.GetComponent<InventarioJogador>();
        TrocarMaterial(materialDesligado);
    }

    void Update()
    {
        // Confere a cada frame se o jogador já juntou Rocklets suficientes.
        if (aberto == false)
        {
            if (inventario.Rocklets >= metaRocklets)
            {
                AbrirPortal();
            }
        }
    }

    void AbrirPortal()
    {
        aberto = true;
        TrocarMaterial(materialLigado);
        jogador.TocarSom(somAbrir, 1f);
        jogador.MostrarMensagem("Portal ligado! Vá até ele.");
    }

    // Troca o material de todas as partes do portal.
    void TrocarMaterial(Material novo)
    {
        if (novo == null)
        {
            return;
        }

        for (int i = 0; i < partesPortal.Length; i++)
        {
            partesPortal[i].material = novo;
        }
    }

    void OnTriggerEnter(Collider outro)
    {
        RespawnJogador quemEntrou = outro.GetComponent<RespawnJogador>();
        if (quemEntrou == null)
        {
            return;
        }
        if (usado == true)
        {
            return;
        }

        if (aberto == false)
        {
            int faltam = metaRocklets - inventario.Rocklets;
            jogador.MostrarMensagem("Faltam " + faltam + " Rocklets para ligar o portal!");
        }
        else
        {
            usado = true;
            if (hud != null)
            {
                hud.PararTempo();
            }
            jogador.MostrarMensagem("Fase concluída!");
            Invoke("CarregarProximaFase", 2f);
        }
    }

    void CarregarProximaFase()
    {
        if (proximaCena != "")
        {
            SceneManager.LoadScene(proximaCena);
        }
        else
        {
            Debug.Log("Fase concluída! Escreva o nome da próxima cena no PortalFase.");
        }
    }
}
