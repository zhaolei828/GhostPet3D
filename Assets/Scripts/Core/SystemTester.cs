using UnityEngine;

/// <summary>
/// 系统测试器 - 用于直接测试伤害、死亡、重生功能
/// </summary>
public class SystemTester : MonoBehaviour
{
    private float testTimer = 0f;
    
    private void Update()
    {
        testTimer += Time.deltaTime;
        
        // 每3秒测试一次功能
        if (testTimer >= 3f)
        {
            testTimer = 0f;
            TestAllSystems();
        }
    }
    
    private void TestAllSystems()
    {
        Debug.Log("[SystemTester] ============ 开始测试所有系统 ============");
        
        // 测试1：给敌人造成伤害
        TestEnemyDamage();
        
        // 测试2：如果敌人都死了，测试玩家死亡
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            TestPlayerDeath();
        }
    }
    
    private void TestEnemyDamage()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            HealthSystem enemyHealth = enemies[0].GetComponent<HealthSystem>();
            if (enemyHealth != null && enemyHealth.IsAlive)
            {
                Debug.Log($"[SystemTester] 给敌人 {enemies[0].name} 造成25点伤害！");
                enemyHealth.TakeDamage(25f);
            }
        }
        else
        {
            Debug.Log("[SystemTester] 没有找到敌人，测试敌人伤害功能跳过");
        }
    }
    
    private void TestPlayerDeath()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null && playerHealth.IsAlive)
            {
                Debug.Log($"[SystemTester] 给玩家造成致命伤害，测试重生系统！");
                playerHealth.TakeDamage(999f); // 致命伤害
            }
        }
        else
        {
            Debug.Log("[SystemTester] 没有找到玩家");
        }
    }
}
