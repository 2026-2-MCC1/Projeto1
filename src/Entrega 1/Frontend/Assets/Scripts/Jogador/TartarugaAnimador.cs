using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Liga o movimento do jogador (CharacterController) às animações da tartaruga
[RequireComponent(typeof(Animator))]
public class TartarugaAnimador : MonoBehaviour
{
    // Tecla que dispara a animação de giro (só usada se o AtaqueJogador não existir)
    public Key teclaGiro = Key.Q;

    // Velocidade de caminhada usada como referência para o ritmo da animação
    public float velocidadeBase = 5f;

    // Tempo no ar (em segundos) antes de considerar que o jogador está pulando
    public float tempoParaAr = 0.08f;

    // Referência ao Animator da tartaruga
    private Animator animator;

    // Referência ao CharacterController do jogador (objeto pai)
    private CharacterController controller;

    // Guarda há quanto tempo o jogador está fora do chão
    private float tempoNoAr = 0f;

    // Duração da reação de dano (igual ao clipe Tartaruga_Dano)
    public float duracaoDano = 0.55f;
    private float tempoDano = 0f;

    // Morte: espera a tartaruga cair no chão e depois pisca até sumir
    public float esperaAntesDePiscar = 1.3f;
    public float tempoPiscando = 1.4f;
    private bool morto = false;

    // Tempo total da morte (queda + piscar), usado para abrir o Game Over depois
    public float DuracaoMorte { get { return esperaAntesDePiscar + tempoPiscando; } }

    private ArcorFactory.AtaqueJogador ataque;
    private bool temParametroGirando;
    private bool giroPendente;
    private static readonly int HashGiro = Animator.StringToHash("Giro");
    private static readonly int HashParamGirando = Animator.StringToHash("Girando");

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == "Girando") temParametroGirando = true;
        }
    }

    void Start()
    {
        ConectarAtaque();
    }

    void ConectarAtaque()
    {
        if (ataque != null) return;
        ataque = GetComponentInParent<ArcorFactory.AtaqueJogador>();
        if (ataque == null) return;
        ataque.AoComecarGiro += AoComecarGiro;

        // a trava dura exatamente o clipe do giro
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null && clip.name == "Tartaruga_Giro")
                {
                    ataque.DefinirDuracaoGiro(clip.length);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (ataque != null) ataque.AoComecarGiro -= AoComecarGiro;
    }

    // Um giro aceito = um trigger
    void AoComecarGiro()
    {
        if (morto || tempoDano > 0f) return;
        animator.ResetTrigger("Girar");
        animator.SetTrigger("Girar");
        giroPendente = true;
    }

    // O Animator está no estado Giro, ou entrando nele
    bool AnimacaoGiroAtiva()
    {
        if (giroPendente) return true;
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashGiro) return true;
        return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == HashGiro;
    }

    void Update()
    {
        if (controller == null)
        {
            return;
        }
        ConectarAtaque();

        // o trigger já virou estado: limpa para não sobrar um giro "guardado"
        if (giroPendente && (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == HashGiro
            || (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == HashGiro)))
        {
            giroPendente = false;
            animator.ResetTrigger("Girar");
        }
        if (ataque != null && !ataque.Girando && giroPendente)
        {
            // o giro foi encerrado sem a animação entrar (golpe, pausa): descarta o pedido
            giroPendente = false;
            animator.ResetTrigger("Girar");
        }
        if (ataque != null)
        {
            ataque.animacaoGiroAtiva = AnimacaoGiroAtiva();
            ataque.animacaoBloqueada = morto || tempoDano > 0f;
            if (temParametroGirando)
            {
                animator.SetBool(HashParamGirando, ataque.Girando);
            }
        }

        // Morta: segura a animação de morte sem pular, andar ou girar
        if (morto)
        {
            animator.SetFloat("Velocidade", 0f);
            animator.SetBool("NoChao", true);
            return;
        }

        // Velocidade horizontal do jogador (ignora a queda/pulo)
        Vector3 velocidade = controller.velocity;
        velocidade.y = 0f;
        float velocidadeAtual = velocidade.magnitude;

        // Conta o tempo no ar para evitar tremida quando o chão falha por um frame
        if (controller.isGrounded)
        {
            tempoNoAr = 0f;
        }
        else
        {
            tempoNoAr += Time.deltaTime;
        }

        bool noChao = tempoNoAr < tempoParaAr;

        // Enquanto reage ao golpe, segura o pulo e o giro para a animação não ser cortada
        if (tempoDano > 0f)
        {
            tempoDano -= Time.deltaTime;
            noChao = true;
        }

        animator.SetFloat("Velocidade", velocidadeAtual, 0.05f, Time.deltaTime);
        animator.SetFloat("MultAndar", Mathf.Clamp(velocidadeAtual / velocidadeBase, 0.8f, 2f));
        animator.SetBool("NoChao", noChao);

        // Sem o AtaqueJogador (cena antiga)
        if (ataque == null && noChao && tempoDano <= 0f && Keyboard.current != null && Keyboard.current[teclaGiro].wasPressedThisFrame
            && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != HashGiro)
        {
            animator.ResetTrigger("Girar");
            animator.SetTrigger("Girar");
        }
    }

    // Chamado pelo InventarioJogador quando a tartaruga perde um casco
    public void LevarDano()
    {
        if (morto)
        {
            return;
        }

        animator.ResetTrigger("Girar");
        giroPendente = false;
        if (ataque != null) ataque.CancelarGiro();
        animator.SetTrigger("Dano");
        tempoDano = duracaoDano;
    }

    // Chamado pelo InventarioJogador quando acabam os cascos
    public void Morrer()
    {
        if (morto)
        {
            return;
        }
        morto = true;

        animator.ResetTrigger("Girar");
        giroPendente = false;
        if (ataque != null) ataque.CancelarGiro();
        animator.ResetTrigger("Dano");
        animator.SetTrigger("Morrer");

        StartCoroutine(PiscarAteSumir());
    }

    // Depois de cair, a tartaruga pisca cada vez mais rápido e some
    IEnumerator PiscarAteSumir()
    {
        yield return new WaitForSeconds(esperaAntesDePiscar);

        Renderer[] partes = GetComponentsInChildren<Renderer>();
        bool visivel = true;
        float passou = 0f;

        while (passou < tempoPiscando)
        {
            float intervalo = Mathf.Lerp(0.16f, 0.04f, passou / tempoPiscando);
            visivel = !visivel;
            foreach (Renderer parte in partes)
            {
                parte.enabled = visivel;
            }

            yield return new WaitForSeconds(intervalo);
            passou += intervalo;
        }

        foreach (Renderer parte in partes)
        {
            parte.enabled = false;
        }
    }
}
