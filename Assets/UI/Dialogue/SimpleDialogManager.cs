using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Choice { public string choiceText; public string nextNodeId; }

[System.Serializable]
public class DialogueNode { public string nodeId; public string npcText; public Choice[] choices; }

[System.Serializable]
public class DialogueEntry { public string npcName; public DialogueNode[] nodes; }

[System.Serializable]
public class DialogueDatabase { public DialogueEntry[] dialogues; }

public class SimpleDialogManager : MonoBehaviour
{
    public TextAsset jsonFile;
    public GameObject painelUI;
    public TMP_Text textoDoDialogo;

    [Header("Configuração Dinâmica de Botões")]
    [Tooltip("Arraste o Prefab do seu botão aqui")]
    public GameObject prefabBotao;
    [Tooltip("Arraste o objeto/painel que vai segurar os botões aqui")]
    public Transform localDosBotoes;

    private Dictionary<string, DialogueNode[]> bancoDeDialogos = new Dictionary<string, DialogueNode[]>();
    private DialogueNode[] nosAtuaisDoNPC;

    void Start()
    {
        DialogueDatabase db = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text);
        foreach (var entry in db.dialogues)
        {
            bancoDeDialogos[entry.npcName] = entry.nodes;
        }
        painelUI.SetActive(false);
    }

    public void IniciarDialogo(string nomeDoNPC)
    {
        if (bancoDeDialogos.ContainsKey(nomeDoNPC))
        {
            nosAtuaisDoNPC = bancoDeDialogos[nomeDoNPC];
            painelUI.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            IrParaNo("inicio");
        }
    }

    public void IrParaNo(string idDoNo)
    {
        DialogueNode noAlvo = null;
        foreach (var no in nosAtuaisDoNPC)
        {
            if (no.nodeId == idDoNo) noAlvo = no;
        }

        if (noAlvo == null) return;

        textoDoDialogo.text = noAlvo.npcText;

        // 1. Limpa os botões antigos da tela antes de criar os novos
        LimparBotoesAntigos();

        // 2. Se não tiver escolhas, cria apenas o botão "Sair"
        if (noAlvo.choices == null || noAlvo.choices.Length == 0)
        {
            CriarBotao("Sair", FecharDialogo);
            return;
        }

        // 3. Instancia um botão para cada escolha no JSON
        for (int i = 0; i < noAlvo.choices.Length; i++)
        {
            string textoDaEscolha = noAlvo.choices[i].choiceText;
            string proximoId = noAlvo.choices[i].nextNodeId; // Captura local necessária para o C#

            // Cria o botão passando o texto e a ação que ele deve executar
            CriarBotao(textoDaEscolha, () => IrParaNo(proximoId));
        }
    }

    // Função auxiliar que cria o botão fisicamente na tela
    private void CriarBotao(string texto, UnityEngine.Events.UnityAction acao)
    {
        // Instancia o prefab como "filho" do localDosBotoes
        GameObject novoBotao = Instantiate(prefabBotao, localDosBotoes);

        // Altera o texto
        novoBotao.GetComponentInChildren<TextMeshProUGUI>().text = texto;

        // Configura o clique
        Button btn = novoBotao.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(acao);
    }

    // Função que destroi os botões gerados na fala anterior
    private void LimparBotoesAntigos()
    {
        foreach (Transform filho in localDosBotoes)
        {
            Destroy(filho.gameObject);
        }
    }

    public void FecharDialogo()
    {
        LimparBotoesAntigos(); // Limpa a tela ao fechar
        painelUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}

