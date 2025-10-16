using System.Collections.Generic;
using UnityEngine;

public class PeelStampPool : MonoBehaviour
{
    public static PeelStampPool Instance;

    public SpriteMask stampPrefab;   // prefab SpriteMask (viên nang/rect bo tròn)
    public int prewarmCount = 100;   // số lượng tạo sẵn
    public Transform poolRoot;       // nơi chứa tem nhàn rỗi (để trống sẽ dùng chính transform)

    readonly Queue<SpriteMask> pool = new Queue<SpriteMask>();

    void Awake()
    {
        Instance = this;
        if (!poolRoot) poolRoot = transform;

        // prewarm
        for (int i = 0; i < Mathf.Max(0, prewarmCount); i++)
        {
            var s = Instantiate(stampPrefab, poolRoot);
            s.gameObject.SetActive(false);
            pool.Enqueue(s);
        }
    }

    public SpriteMask Rent()
    {
        SpriteMask s = pool.Count > 0 ? pool.Dequeue()
                                      : Instantiate(stampPrefab, poolRoot);
        s.gameObject.SetActive(true);
        // KHÔNG parent theo quả; để dưới poolRoot (global) như yêu cầu
        return s;
    }

    public void Return(SpriteMask s)
    {
        if (!s) return;
        s.gameObject.SetActive(false);
        s.transform.SetParent(poolRoot, worldPositionStays: true);
        pool.Enqueue(s);
    }
}
