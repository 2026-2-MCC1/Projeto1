using UnityEngine;
using UnityEngine.SceneManagement;

// Vai no Player.
// Guarda o último checkpoint, cuida das vidas quando o jogador leva dano
// e tem dois métodos que os outros scripts usam: TocarSom() e MostrarMensagem().
public class RespawnJogador : MonoBehaviour
{
    public Transform pontoInicial;        // onde o jogador volta antes de pegar algum checkpoint
    public float limiteQueda = -9.5f;     // se o jogador ficar abaixo dessa altura, ele caiu
    public bool quedaTiraVida = true;     // cair no líquido também tira uma vida (desmarque para só voltar ao checkpoint)
    public MensagemUI mensagem;           // o script que mostra mensagens na tela

    private Vector3 pontoAtual;           // posição do último checkpoint
    private CharacterController controle;
    private InventarioJogador inventario;
    private AudioSource fonteSom;
    private bool fimDeJogo = false;

    void Start()
    {
        controle = GetComponent<CharacterController>();
        inventario = GetComponent<InventarioJogador>();

        // Se a mensagem não foi colocada no Inspector, procura na cena.
        if (mensagem == null)
        {
            mensagem = FindFirstObjectByType<MensagemUI>();
        }

        // Fonte de som do jogador (som 2D: mesmo volume em qualquer lugar).
        fonteSom = GetComponent<AudioSource>();
        if (fonteSom == null)
        {
            fonteSom = gameObject.AddComponent<AudioSource>();
        }
        fonteSom.spatialBlend = 0f;
        fonteSom.playOnAwake = false;

        // Começa usando o ponto inicial. Se ele não foi colocado, usa a posição atual do jogador.
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

    // Chamado pelo Perigo: volta para o checkpoint e perde uma vida.
    public void LevarDano()
    {
        VoltarAoCheckpoint();

        // Se as vidas já acabaram, não tira mais nenhuma.
        if (fimDeJogo == true)
        {
            return;
        }

        if (inventario != null)
        {
            inventario.PerderVida();

            if (inventario.Vidas <= 0)
            {
                fimDeJogo = true;
                MostrarMensagem("Suas vidas acabaram! Recomeçando a fase...");
                Invoke("ReiniciarFase", 2f);
            }
        }
    }

    // Leva o jogador para o último checkpoint.
    public void VoltarAoCheckpoint()
    {
        // O CharacterController precisa ser desligado para mudar a posição do jogador.
        controle.enabled = false;
        transform.position = pontoAtual;
        controle.enabled = true;
    }

    // Chamado pelo checkpoint quando o jogador passa por ele.
    public void SalvarCheckpoint(Vector3 novoPonto)
    {
        pontoAtual = novoPonto;
    }

    // Toca um som no jogador. Qualquer script pode chamar.
    public void TocarSom(AudioClip som, float volume)
    {
        if (som != null)
        {
            fonteSom.PlayOneShot(som, volume);
        }
    }

    // Mostra uma mensagem na tela. Qualquer script pode chamar.
    public void MostrarMensagem(string texto)
    {
        if (mensagem != null)
        {
            mensagem.MostrarMensagem(texto);
        }
    }

    void ReiniciarFase()
    {
        // Carrega a cena atual de novo: vidas, Rocklets e checkpoints voltam ao começo.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
