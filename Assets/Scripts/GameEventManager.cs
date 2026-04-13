using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    // Inscreve-se no evento quando o objeto é ativado
    private void OnEnable()
    {
        SimpleDialogManager.AoDispararAcaoDeDialogo += ProcessarAcao;
    }

    // Desinscreve-se quando desativado para evitar vazamento de memória (Memory Leak)
    private void OnDisable()
    {
        SimpleDialogManager.AoDispararAcaoDeDialogo -= ProcessarAcao;
    }

    // Esta função roda automaticamente quando o diálogo encontra uma "acaoGatilho"
    private void ProcessarAcao(string idDaAcao)
    {
        switch (idDaAcao)
        {
            case "dar_pocao":
                Debug.Log("Sistema de Inventário: Adicionou 1 Poção.");
                break;
            case "tocar_musica_triste":
                Debug.Log("Sistema de Áudio: Trocando trilha sonora.");
                break;
            default:
                Debug.LogWarning($"Ação '{idDaAcao}' recebida, mas não há lógica associada.");
                break;
        }
    }
}