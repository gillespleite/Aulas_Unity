// Importação de bibliotecas essenciais
using System.Collections.Generic; // Necessário para o uso de Listas e Dicionários
using UnityEngine; // Biblioteca principal da engine Unity
using UnityEngine.UI; // Necessário para interagir com componentes clássicos de UI (como Button)
using TMPro; // Necessário para utilizar a TextMeshPro, sistema avançado de renderização de texto
using System;


// As classes abaixo definem a estrutura de dados que espelha o arquivo JSON.
// O atributo [System.Serializable] permite que o JsonUtility converta o texto JSON nesses objetos.
[System.Serializable]
public class DialogueEntry { public string npcName; public DialogueNode[] nodes; }

[System.Serializable]
public class DialogueDatabase { public DialogueEntry[] dialogues; }

[System.Serializable]
public class Choice { public string choiceText; public string nextNodeId; }

[System.Serializable]
public class DialogueNode { public string nodeId; public string npcText; public string acaoGatilho; public Choice[] choices; }

public class SimpleDialogManager : MonoBehaviour
{

    public static event Action<string> AoDispararAcaoDeDialogo;
    // Define qual idioma será carregado (0 ou 1). Fica visível no Inspector.
    [SerializeField]
    int lingua;

    // Variáveis públicas para associar referências arrastando no Inspector da Unity.
    public TextAsset jsonFile; // O arquivo de texto bruto do JSON.
    public GameObject painelUI; // O painel pai que contém toda a interface de diálogo.
    public TMP_Text textoDoDialogo; // O componente de texto onde a fala do NPC aparece.
    public TMP_Text nomeNPC; // O componente de texto que exibe o nome do NPC.
    

    // Cabeçalho para organizar o Inspector.
    [Header("Configuração Dinâmica de Botões")]
    public GameObject prefabBotao; // O modelo (prefab) do botão de escolha a ser instanciado.
    public Transform localDosBotoes; // O objeto pai (Layout Group) onde os botões serão organizados.

    // Dicionário aninhado para acesso rápido (O(1)). 
    // Mapeia o Nome do NPC -> Dicionário de Nós (ID do Nó -> Dados do Nó).
    private Dictionary<string, Dictionary<string, DialogueNode>> bancoDeDialogosOtimizado = new Dictionary<string, Dictionary<string, DialogueNode>>();

    // Referência em cache para o dicionário de falas do NPC com o qual estamos conversando no momento.
    private Dictionary<string, DialogueNode> nosAtuaisDoNPC;

    // Lista que atua como um Object Pool para reutilizar botões e evitar sobrecarga de memória (Garbage Collection).
    private List<GameObject> poolDeBotoes = new List<GameObject>();

    // Método nativo da Unity chamado antes do primeiro frame. Está vazio e consumindo micro-processamento desnecessário.
    private void Awake()
    {

    }

    // Método nativo da Unity chamado no primeiro frame do objeto ativo.
    void Start()
    {
        // Executa a leitura e organização dos dados apenas uma vez ao iniciar o jogo.
        CarregaDialogos();
    }

    // Função responsável por ler o arquivo JSON e popular os dicionários em memória.
    void CarregaDialogos()
    {
        // Verifica o número inteiro escolhido para o idioma.
        switch (lingua)
        {
            case 0:
                // Carrega o arquivo dinamicamente da pasta "Resources" da Unity (caminho: Resources/Dialogues_PT).
                jsonFile = Resources.Load<TextAsset>("Dialogues_PT");
                break;

            case 1:
                jsonFile = Resources.Load<TextAsset>("Dialogues_EN");
                break;
        }

        // Converte o texto bruto do JSON instanciando a estrutura DialogueDatabase.
        DialogueDatabase db = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text);

        // Itera sobre todos os diálogos presentes no banco de dados lido.
        foreach (var entry in db.dialogues)
        {
            // Cria um dicionário temporário para armazenar os nós específicos deste NPC.
            var dicionarioDeNos = new Dictionary<string, DialogueNode>();

            // Itera sobre todas as falas (nós) do NPC atual.
            foreach (var node in entry.nodes)
            {
                // Guarda o nó no dicionário, usando o ID do nó como chave de busca.
                dicionarioDeNos[node.nodeId] = node;
            }
            // Guarda o dicionário de nós preenchido dentro do dicionário principal, usando o nome do NPC como chave.
            bancoDeDialogosOtimizado[entry.npcName] = dicionarioDeNos;
        }

        // Garante que a interface de diálogo comece escondida.
        painelUI.SetActive(false);
    }

    // Inicia a conversa com um NPC específico.
    public void IniciarDialogo(string nomeDoNPC)
    {
        // Tenta buscar as falas do NPC solicitado no dicionário. Se encontrar, armazena em 'nosAtuaisDoNPC'.
        if (bancoDeDialogosOtimizado.TryGetValue(nomeDoNPC, out nosAtuaisDoNPC))
        {
          
            nomeNPC.text = nomeDoNPC; // Atualiza o texto da UI com o nome do NPC.
            painelUI.SetActive(true); // Exibe a interface de diálogo.
            Cursor.visible = true; // Torna o cursor do mouse visível.
            Cursor.lockState = CursorLockMode.None; // Destrava o cursor para permitir cliques na interface.

            IrParaNo("inicio"); // Pula automaticamente para a fala de ID "inicio".
        }
        else
        {
            // Se o NPC não for encontrado no dicionário, emite um alerta no console para debug.
            Debug.LogWarning($"NPC {nomeDoNPC} não encontrado no banco de dados.");
        }
    }

    // Atualiza a interface exibindo o texto do nó atual e criando os botões de escolha.
    public void IrParaNo(string idDoNo)
    {
        // Tenta buscar o nó de destino no dicionário temporário do NPC atual.
        if (!nosAtuaisDoNPC.TryGetValue(idDoNo, out DialogueNode noAlvo))
        {
            // Se o ID não existir, emite erro e interrompe a execução da função.
            Debug.LogError($"Nó '{idDoNo}' não encontrado!");
            return;
        }

       // Verifica se o nó atual possui uma ação registrada e dispara o evento
    if (!string.IsNullOrEmpty(noAlvo.acaoGatilho))
        {
            // Se alguém estiver escutando o evento (? invoca), manda a string da ação
            AoDispararAcaoDeDialogo?.Invoke(noAlvo.acaoGatilho);
        }
        // Atualiza a interface com a fala do NPC.
        textoDoDialogo.text = noAlvo.npcText;

        // Limpa a tela desativando as opções da fala anterior.
        DesativarBotoes();

        // Verifica se chegamos ao fim do diálogo (array de escolhas nulo ou vazio).
        if (noAlvo.choices == null || noAlvo.choices.Length == 0)
        {
            // Gera um único botão para fechar o diálogo.
            ConfigurarBotao(0, "Sair", FecharDialogo);
            return;
        }

        // Se houverem opções de resposta, itera sobre elas.
        for (int i = 0; i < noAlvo.choices.Length; i++)
        {
            string textoDaEscolha = noAlvo.choices[i].choiceText; // Pega o texto da opção.
            string proximoId = noAlvo.choices[i].nextNodeId; // Pega o ID de destino desta opção.

            // Configura um botão da pool para exibir o texto e, ao ser clicado, pular para o próximo ID.
            ConfigurarBotao(i, textoDaEscolha, () => IrParaNo(proximoId));
        }
    }

    // Configura fisicamente o botão da interface, resgatando do pool ou criando um novo.
    private void ConfigurarBotao(int indice, string texto, UnityEngine.Events.UnityAction acao)
    {
        GameObject botaoObj;

        // Se o índice atual exceder o tamanho do nosso pool, precisamos criar (Instanciar) um novo botão.
        if (indice >= poolDeBotoes.Count)
        {
            botaoObj = Instantiate(prefabBotao, localDosBotoes); // Cria o botão na hierarquia definida.
            poolDeBotoes.Add(botaoObj); // Adiciona o novo botão à lista para reúso futuro.
        }
        else
        {
            // Se já tivermos botões suficientes inativos, apenas pegamos o existente.
            botaoObj = poolDeBotoes[indice];
        }

        botaoObj.SetActive(true); // Garante que o objeto do botão esteja visível.

        // Busca o componente de texto dentro do botão e altera seu valor. (Custo de processamento alto).
        botaoObj.GetComponentInChildren<TextMeshProUGUI>().text = texto;

        Button btn = botaoObj.GetComponent<Button>(); // Busca o componente de clique do botão.
        btn.onClick.RemoveAllListeners(); // Limpa ações antigas amarradas a este botão pelo uso anterior.
        btn.onClick.AddListener(acao); // Adiciona a nova ação de clique definida por quem chamou a função.
    }

    // Desliga todos os botões visíveis da interface em vez de destruí-los da memória.
    private void DesativarBotoes()
    {
        for (int i = 0; i < poolDeBotoes.Count; i++)
        {
            poolDeBotoes[i].SetActive(false); // Esconde o botão.
        }
    }

    // Encerra a conversa e devolve o controle ao jogador.
    public void FecharDialogo()
    {
        DesativarBotoes(); // Oculta todas as escolhas para a próxima vez.
        painelUI.SetActive(false); // Oculta o painel principal.
        Cursor.visible = false; // Esconde o mouse novamente.
        Cursor.lockState = CursorLockMode.Locked; // Trava o mouse no centro da tela (comportamento comum de FPS/TPS).
      
    }

    // Modifica o idioma em tempo de execução e recarrega os dados para a memória.
    public void TrocaLingua(int selection)
    {
        lingua = selection; // Altera a variável de estado do idioma.
        CarregaDialogos(); // Roda todo o processo de carregamento de JSON e estruturação de dicionários novamente.
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