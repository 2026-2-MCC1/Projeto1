using FactoryEscape;
using UnityEngine;
using UnityEngine.SceneManagement;

// Vai no Player
public class RespawnJogador : MonoBehaviour
{
    public Transform pontoInicial;
    public float limiteQueda = -9.5f; // se o jogador ficar abaixo dessa altura
    public bool quedaTiraVida = true; // cair no líquido também tira uma vida
    public MensagemUI mensagem; // o script que mostra mensagens na tela

    private Vector3 pontoAtual; // posição do último checkpoint
    private CharacterController controle;
    private InventarioJogador inventario;
    private AudioSource fonteSom;
    private bool fimDeJogo = false;

    void Start()
    {
        controle = GetComponent<CharacterController>();
        inventario = GetComponent<InventarioJogador>();

        // Se a mensagem não foi colocada no Inspector, procura na cena
        if (mensagem == null)
        {
            mensagem = FindFirstObjectByType<MensagemUI>();
        }

        // Fonte de som do jogador (som 2D: mesmo volume em qualquer lugar)
        fonteSom = GetComponent<AudioSource>();
        if (fonteSom == null)
        {
            fonteSom = gameObject.AddComponent<AudioSource>();
        }
        fonteSom.spatialBlend = 0f;
        fonteSom.playOnAwake = false;

        // Começa usando o ponto inicial
        if (pontoInicial != null)
        {
            pontoAtual = pontoInicial.position;
        }
        else
        {
            pontoAtual = transform.position;
        }
    }

    void Update()
    {
        if (InventarioJogador.Congelado) return;
        if (transform.position.y < limiteQueda)
        {
            if (quedaTiraVida == true)
            {
                LevarDano();
            }
            else
            {
                VoltarAoCheckpoint();
            }
        }
    }

    // Chamado pelo Perigo: volta para o checkpoint e perde uma vida
    public void LevarDano()
    {
        // No golpe que mata, a tartaruga fica onde está para mostrar a animação de morte
        bool caiu = transform.position.y < limiteQueda;
        bool golpeFatal = fimDeJogo
            || (inventario != null && inventario.Vidas <= 1 && !inventario.Invencivel);

        if (caiu || !golpeFatal)
        {
            VoltarAoCheckpoint();
        }

        // Se as vidas já acabaram, não tira mais nenhuma
        if (fimDeJogo == true)
        {
            return;
        }

        if (inventario != null)
        {
            inventario.PerderVida();

            // O InventarioJogador mostra o aviso, toca a morte e abre o Game Over
            if (inventario.Vidas <= 0)
            {
                fimDeJogo = true;
            }
        }
    }

    // Leva o jogador para o último checkpoint
    public void VoltarAoCheckpoint()
    {
        // O CharacterController precisa ser desligado para mudar a posição do jogador
        controle.enabled = false;
        transform.position = pontoAtual;
        controle.enabled = true;
    }

    // Chamado pelo checkpoint quando o jogador passa por ele
    public void SalvarCheckpoint(Vector3 novoPonto)
    {
        pontoAtual = novoPonto;
    }

    // Toca um som no jogador
    public void TocarSom(AudioClip som, float volume)
    {
        if (som != null)
        {
            fonteSom.PlayOneShot(som, volume);
        }
    }

    // Mostra uma mensagem na tela
    public void MostrarMensagem(string texto)
    {
        if (mensagem != null)
        {
            mensagem.MostrarMensagem(texto);
        }
    }

    void IrParaGameOver()
    {
        // Abre a tela de Game Over
        GameFlow.GameOver();
    }
}
