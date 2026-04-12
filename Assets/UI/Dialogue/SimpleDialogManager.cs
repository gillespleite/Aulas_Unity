using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class DialogueEntry { public string npcName; public DialogueNode[] nodes; }

[System.Serializable]
public class DialogueDatabase { public DialogueEntry[] dialogues; }

[System.Serializable]
public class Choice { public string choiceText; public string nextNodeId; }

[System.Serializable]
public class DialogueNode { public string nodeId; public string npcText; public Choice[] choices; }

public class SimpleDialogManager : MonoBehaviour
{

    [SerializeField]
    int lingua;

    public TextAsset jsonFile;
    public GameObject painelUI;
    public TMP_Text textoDoDialogo;

    [Header("Configuração Dinâmica de Botões")]
    public GameObject prefabBotao;
    public Transform localDosBotoes;

    // OTIMIZAÇÃO 1: Dicionário Aninhado. 
    // Chave 1: Nome do NPC. Chave 2: ID do Nó. Valor: Os dados do Nó.
    private Dictionary<string, Dictionary<string, DialogueNode>> bancoDeDialogosOtimizado = new Dictionary<string, Dictionary<string, DialogueNode>>();

    // Memória rápida para o NPC que está falando agora
    private Dictionary<string, DialogueNode> nosAtuaisDoNPC;

    // OTIMIZAÇÃO 2: Cache de Botões (Object Pooling simples)
    private List<GameObject> poolDeBotoes = new List<GameObject>();

    private void Awake()
    {
       
    }
    void Start()
    {

        // Lemos o JSON uma única vez e montamos o super-dicionário
        CarregaDialogos();
    }

    void CarregaDialogos()
    {

        switch (lingua)
        {
            case 0:
                jsonFile = Resources.Load<TextAsset>("Dialogues_PT");
                break;

            case 1:
                jsonFile = Resources.Load<TextAsset>("Dialogues_EN");
                break;
        }
        DialogueDatabase db = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text);

        foreach (var entry in db.dialogues)
        {
            var dicionarioDeNos = new Dictionary<string, DialogueNode>();
            foreach (var node in entry.nodes)
            {
                dicionarioDeNos[node.nodeId] = node;
            }
            bancoDeDialogosOtimizado[entry.npcName] = dicionarioDeNos;
        }

        painelUI.SetActive(false);
    }

    public void IniciarDialogo(string nomeDoNPC)
    {
        if (bancoDeDialogosOtimizado.TryGetValue(nomeDoNPC, out nosAtuaisDoNPC))
        {
            painelUI.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            IrParaNo("inicio");
        }
        else
        {
            Debug.LogWarning($"NPC {nomeDoNPC} não encontrado no banco de dados.");
        }
    }

    public void IrParaNo(string idDoNo)
    {
        // OTIMIZAÇÃO 1 (Prática): Busca instantânea sem 'foreach'
        if (!nosAtuaisDoNPC.TryGetValue(idDoNo, out DialogueNode noAlvo))
        {
            Debug.LogError($"Nó '{idDoNo}' não encontrado!");
            return;
        }

        textoDoDialogo.text = noAlvo.npcText;

        // Desativa todos os botões antes de configurar os novos
        DesativarBotoes();

        if (noAlvo.choices == null || noAlvo.choices.Length == 0)
        {
            ConfigurarBotao(0, "Sair", FecharDialogo);
            return;
        }

        for (int i = 0; i < noAlvo.choices.Length; i++)
        {
            string textoDaEscolha = noAlvo.choices[i].choiceText;
            string proximoId = noAlvo.choices[i].nextNodeId;

            ConfigurarBotao(i, textoDaEscolha, () => IrParaNo(proximoId));
        }
    }

    // OTIMIZAÇÃO 2 (Prática): Reciclando botões
    private void ConfigurarBotao(int indice, string texto, UnityEngine.Events.UnityAction acao)
    {
        GameObject botaoObj;

        // Se o índice for maior que nossa lista, significa que precisamos criar um botão novo
        if (indice >= poolDeBotoes.Count)
        {
            botaoObj = Instantiate(prefabBotao, localDosBotoes);
            poolDeBotoes.Add(botaoObj);
        }
        else
        {
            // Se já temos um botão guardado, apenas o reutilizamos
            botaoObj = poolDeBotoes[indice];
        }

        botaoObj.SetActive(true); // Liga o botão

        // OTIMIZAÇÃO 3: GetComponent consome processamento, evite sempre que possível.
        // O ideal seria um script "DialogButton" no Prefab, mas isso atende bem o propósito.
        botaoObj.GetComponentInChildren<TextMeshProUGUI>().text = texto;

        Button btn = botaoObj.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(acao);
    }

    private void DesativarBotoes()
    {
        // Ao invés de Destroy(), nós apenas "escondemos" os botões na memória
        for (int i = 0; i < poolDeBotoes.Count; i++)
        {
            poolDeBotoes[i].SetActive(false);
        }
    }

    public void FecharDialogo()
    {
        DesativarBotoes();
        painelUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public void TrocaLingua(int selection)
    {
        lingua = selection;
        CarregaDialogos();
    }
}



/* Caso queira typewrithing effect

public void IrParaNo(string idDoNo)
    {
        DialogueNode noAlvo = null;
        foreach (var no in nosAtuaisDoNPC)
        {
            if (no.nodeId == idDoNo) noAlvo = no;
        }

        if (noAlvo == null) return;

        // 1. Limpa os botões antigos da tela imediatamente
        LimparBotoesAntigos();

        // 2. Guarda o nó atual na memória para usarmos no final da animação
        noAtualParaBotoes = noAlvo;

        // 3. Se já tiver um texto sendo digitado, interrompe para não encavalar
        if (rotinaDeDigitacao != null)
        {
            StopCoroutine(rotinaDeDigitacao);
        }

        // 4. Inicia a máquina de escrever passando o texto do NPC
        rotinaDeDigitacao = StartCoroutine(DigitarTexto(noAlvo.npcText));
    }

// Esta é a Coroutine que faz o efeito acontecer ao longo do tempo
    private System.Collections.IEnumerator DigitarTexto(string frase)
    {
        textoDoDialogo.text = ""; // Esvazia a caixa de texto

        // Pega a frase inteira, transforma numa lista de letras, e passa uma por uma
        foreach (char letra in frase.ToCharArray())
        {
            textoDoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadeDaLetra); // Pausa o tempo aqui
        }

        // Quando o loop termina, o texto acabou. Agora mostramos os botões!
        MostrarBotoesDeEscolha();
    }

    // Esta lógica saiu do antigo IrParaNo e veio para cá
    private void MostrarBotoesDeEscolha()
    {
        if (noAtualParaBotoes.choices == null || noAtualParaBotoes.choices.Length == 0)
        {
            CriarBotao("Sair", FecharDialogo);
            return;
        }

        for (int i = 0; i < noAtualParaBotoes.choices.Length; i++)
        {
            string textoDaEscolha = noAtualParaBotoes.choices[i].choiceText;
            string proximoId = noAtualParaBotoes.choices[i].nextNodeId; 

            CriarBotao(textoDaEscolha, () => IrParaNo(proximoId));
        }
    }

*/