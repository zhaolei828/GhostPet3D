using UnityEngine;

/// <summary>
/// 快速伤害数字测试 - 强制显示大号红色伤害数字
/// </summary>
public class QuickDamageTest : MonoBehaviour
{
    private void Start()
    {
        // 延迟2秒后开始测试
        Invoke(nameof(ForceShowDamageNumber), 2f);
    }
    
    private void ForceShowDamageNumber()
    {
        Debug.Log("=== 强制伤害数字测试 ===");
        
        // 在屏幕中心显示大号红色数字
        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 5));
        
        if (DamageNumberManager.Instance != null)
        {
            Debug.Log("正在显示强制伤害数字...");
            DamageNumberManager.Instance.ShowDamageNumber(screenCenter, 999, DamageType.PlayerDamage);
            
            // 再在Player位置显示一个
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 playerPos = player.transform.position + Vector3.up * 3;
                DamageNumberManager.Instance.ShowDamageNumber(playerPos, 888, DamageType.EnemyDamage);
            }
        }
        
        // 每5秒重复测试
        Invoke(nameof(ForceShowDamageNumber), 5f);
    }
}
