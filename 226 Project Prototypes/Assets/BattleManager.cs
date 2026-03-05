using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public Slider playerHealthBar;
    public Slider enemyHealthBar;

    private int playerHealth = 100;
    private int enemyHealth = 100;

    private bool isDefending = false;

    void Start()
    {
        playerHealthBar.maxValue = 100;
        enemyHealthBar.maxValue = 100;

        playerHealthBar.value = playerHealth;
        enemyHealthBar.value = enemyHealth;
    }

    public void Attack()
    {
        if (enemyHealth <= 0 || playerHealth <= 0)
            return;

        int damage = 20;
        enemyHealth -= damage;

        if (enemyHealth < 0)
            enemyHealth = 0;

        enemyHealthBar.value = enemyHealth;

        EnemyTurn();
    }

    public void Defend()
    {
        if (enemyHealth <= 0 || playerHealth <= 0)
            return;

        isDefending = true;
        EnemyTurn();
    }

    void EnemyTurn()
    {
        int enemyDamage = 15;

        if (isDefending)
        {
            enemyDamage /= 2;   // take half damage if defending
            isDefending = false;
        }

        playerHealth -= enemyDamage;

        if (playerHealth < 0)
            playerHealth = 0;

        playerHealthBar.value = playerHealth;
    }
}