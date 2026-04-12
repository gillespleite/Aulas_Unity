using UnityEngine;
using UnityEngine.InputSystem;

public class Atirar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Configurações de Tiro")]
    [SerializeField] private GameObject projectilePrefab; // Arraste o Prefab da bala aqui
    [SerializeField] private Transform firePoint;       // Arraste um Objeto Vazio (ponta da arma) aqui
    [SerializeField] private float projectileSpeed = 20f;

    // Este método será chamado pelo Input System (Player Input -> OnFire)
    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // 1. Instancia o projétil na posição e ROTAÇÃO do firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 2. Pega o Rigidbody do projétil para dar velocidade
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Move o projétil para a frente (forward) em relação à sua própria rotação
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }


    }
}
