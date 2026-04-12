using UnityEngine;

public class NPC : MonoBehaviour
{
    string npcName; // Escreva exatamente como está no JSON

    public SimpleDialogManager gerenciador; // Arraste o Canvas/Gerenciador aqui

    private bool jogadorEstaPerto = false;


    private void Start()
    {
        npcName = gameObject.name;
    }

    public void Interact()
    {


        gerenciador.IniciarDialogo(npcName);

    }
}