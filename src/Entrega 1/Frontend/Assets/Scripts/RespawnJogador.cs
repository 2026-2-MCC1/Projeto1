
using System.Collections;
using UnityEngine;
using FactoryEscape;

public class RespawnJogador : MonoBehaviour
{
    public Transform pontoInicial;
    public float limiteQueda = -9.5f;
    public bool quedaTiraVida = true;
    public MensagemUI mensagem;
    public float tempoInvulneravel = 1.5f;

    private Vector3 pontoAtual;
    private CharacterController controle;
    private InventarioJogador inventario;
    private AudioSource fonteSom;
    private bool fimDeJogo = false;
    private float invulneravelAte = 0f;
    private Coroutine empurrao;

    void Start()
    {
        controle = GetComponent<CharacterController>();
        inventario = GetComponent<InventarioJogador>();

        if (mensagem == null)
            mensagem = FindFirstObjectByType<MensagemUI>();

        fonteSom = GetComponent<AudioSource>();

        if (fonteSom == null)
            fonteSom = gameObject.AddComponent<AudioSource>();

        fonteSom.spatialBlend = 0f;
        fonteSom.playOnAwake = false;

        if (pontoInicial != null)
            pontoAtual = pontoInicial.position;
        else
            pontoAtual = transform.position;

        if (inventario == null)
        {
            Debug.LogWarning(
                "[RespawnJogador] O Player NÃO tem o componente InventarioJogador."
            );
        }
    }

    void Update()
    {
        if (transform.position.y < limiteQueda)
        {
            if (quedaTiraVida)
                LevarDano();
            else
                VoltarAoCheckpoint();
        }
    }

    public void TomarDano()
    {
        if (fimDeJogo || Time.time < invulneravelAte)
            return;

        if (inventario == null)
            return;

        invulneravelAte = Time.time + tempoInvulneravel;
        inventario.PerderVida();

        if (inventario.Vidas <= 0)
        {
            fimDeJogo = true;
            IrParaGameOver();
        }
        else
        {
            MostrarMensagem(
                "Você perdeu uma vida! Restam " + inventario.Vidas
            );
        }
    }

    public void LevarDano()
    {
        VoltarAoCheckpoint();

        if (fimDeJogo)
            return;

        invulneravelAte = Time.time + tempoInvulneravel;

        if (inventario != null)
        {
            inventario.PerderVida();

            if (inventario.Vidas <= 0)
            {
                fimDeJogo = true;
                IrParaGameOver();
            }
        }
    }

    public void Empurrar(Vector3 direcao, float forca, float duracao)
    {
        if (fimDeJogo || controle == null || !controle.enabled)
        {
            Debug.LogWarning(
                "[RespawnJogador] Não foi possível aplicar o empurrão."
            );
            return;
        }

        direcao.y = 0f;

        if (direcao.sqrMagnitude < 0.0001f)
            return;

        if (empurrao != null)
            StopCoroutine(empurrao);

        Debug.Log("Empurrão iniciado! Força: " + forca);

        empurrao = StartCoroutine(
            AnimarEmpurrao(direcao.normalized, forca, duracao)
        );
    }

    IEnumerator AnimarEmpurrao(
        Vector3 direcao,
        float forca,
        float duracao
    )
    {
        PlayerMovement movimento = GetComponent<PlayerMovement>();

        if (movimento != null)
            movimento.DefinirEmpurrao(true);

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float restante = 1f - Mathf.Clamp01(tempo / duracao);

            if (controle.enabled)
            {
                controle.Move(
                    direcao * forca * restante * Time.deltaTime
                );
            }

            yield return null;
        }

        if (movimento != null)
            movimento.DefinirEmpurrao(false);

        empurrao = null;
    }

    public void VoltarAoCheckpoint()
    {
        if (empurrao != null)
        {
            StopCoroutine(empurrao);
            empurrao = null;
        }

        PlayerMovement movimento = GetComponent<PlayerMovement>();

        if (movimento != null)
            movimento.DefinirEmpurrao(false);

        if (controle == null)
            return;

        controle.enabled = false;
        transform.position = pontoAtual;
        controle.enabled = true;
    }

    public void SalvarCheckpoint(Vector3 novoPonto)
    {
        pontoAtual = novoPonto;
    }

    public void TocarSom(AudioClip som, float volume)
    {
        if (som != null)
            fonteSom.PlayOneShot(som, volume);
    }

    public void MostrarMensagem(string texto)
    {
        if (mensagem != null)
            mensagem.MostrarMensagem(texto);
    }

    void IrParaGameOver()
    {
        GameFlow.GameOver();
    }
}
