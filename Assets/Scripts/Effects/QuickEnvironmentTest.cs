using UnityEngine;

public class QuickEnvironmentTest : MonoBehaviour
{
    private void Start()
    {
        CreateTestDecorations();
    }
    
    private void CreateTestDecorations()
    {
        // 加载材质
        Material grassMaterial = Resources.Load<Material>("Environment/SnowGrassMaterial");
        Material blackRockMaterial = Resources.Load<Material>("Environment/BlackRockMaterial");  
        Material grayRockMaterial = Resources.Load<Material>("Environment/GrayRockMaterial");
        
        Debug.Log($"[QuickTest] 材质加载: 草={grassMaterial != null}, 黑岩={blackRockMaterial != null}, 灰岩={grayRockMaterial != null}");
        
        // 创建几个测试装饰物
        CreateTestGrass(new Vector3(3, 0.15f, 2), grassMaterial);
        CreateTestGrass(new Vector3(-2, 0.15f, 4), grassMaterial);
        CreateTestGrass(new Vector3(1, 0.15f, -3), grassMaterial);
        
        CreateTestRock(new Vector3(4, 0.15f, -1), blackRockMaterial, "BlackRock");
        CreateTestRock(new Vector3(-3, 0.15f, 1), grayRockMaterial, "GrayRock");
        CreateTestRock(new Vector3(0, 0.15f, 5), blackRockMaterial, "BlackRock");
        
        Debug.Log("[QuickTest] 测试装饰物创建完成！");
    }
    
    private void CreateTestGrass(Vector3 position, Material material)
    {
        GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "TestGrass";
        grass.transform.position = position;
        grass.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
        grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        if (material != null)
        {
            grass.GetComponent<MeshRenderer>().material = material;
        }
        
        // 移除碰撞体
        Destroy(grass.GetComponent<BoxCollider>());
    }
    
    private void CreateTestRock(Vector3 position, Material material, string name)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = name;
        rock.transform.position = position;
        rock.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
        rock.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), Random.Range(-10, 10));
        
        if (material != null)
        {
            rock.GetComponent<MeshRenderer>().material = material;
        }
    }
}
