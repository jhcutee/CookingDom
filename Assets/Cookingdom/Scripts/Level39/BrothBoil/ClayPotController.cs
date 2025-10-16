using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClayPotController : MonoBehaviour
{
    public static ClayPotController Instance;

    [Header("Elements")]
    [SerializeField] private ClayPot clayPot;
    public ClayPot ClayPot => clayPot;

    [SerializeField] private GasStoveKnob gasKnob;

    [SerializeField] private GameObject waterGO;    // Animator trong editor
    [SerializeField] private GameObject brothGO;
    [SerializeField] private GameObject frothGO;// chỉ bật là chạy anim default
    [SerializeField] private Transform potContentRoot;

    [Header("Step 1 Ingredients")]
    [SerializeField] private List<GameObject> requiredIngredientsStep1 = new(); // Pepper, LemonLeaf, Beef
    [SerializeField] private List<GameObject> currentIngredientsStep1 = new();

    [Header("Drop→Sink Settings")]
    [SerializeField] private float sinkDelay = 0.30f;
    [SerializeField] private int underWaterSortingOrder = 3; // water order = 4

    [Header("Boil Bob Settings")]
    [SerializeField] private float bobAmplitude = 0.03f;
    [SerializeField] private float bobSpeed = 2.0f;

    [Header("State")]
    public bool IsStep1Completed = false;
    bool isCookingStep1Running = false;
    bool allowBobbing = false;

    readonly Dictionary<GameObject, Coroutine> _bobCo = new();

    void Awake()
    {
        Instance = this;
        if (!potContentRoot) potContentRoot = clayPot ? clayPot.transform : transform;
    }

    void OnEnable()
    {
        if (gasKnob) gasKnob.turnOn += OnGasTurnOn; // chỉ bật 1 lần
    }
    void OnDisable()
    {
        if (gasKnob) gasKnob.turnOn -= OnGasTurnOn;
    }

    // ---------- Accept / Add ----------
    public bool CanAcceptIngredient(GameObject ingredient)
    {
        if (IsStep1Completed) return false;
        return requiredIngredientsStep1.Contains(ingredient) &&
               !currentIngredientsStep1.Contains(ingredient);
    }

    public void AddIngredientStep1(GameObject ingredient)
    {
        if (CanAcceptIngredient(ingredient))
            currentIngredientsStep1.Add(ingredient);
    }

    public void OnIngredientDropped(GameObject ingredient)
    {
        var t = ingredient.transform;
        if (!IsStep1Completed)
        {
            ToogleDraggable(ingredient, IsStep1Completed);
        }
        t.SetParent(potContentRoot, false);
        t.localPosition = Vector3.zero; // snap về tâm nồi
        StartCoroutine(CoSinkThenMaybeBob(ingredient));
    }

    // ---------- Sink then Bob ----------
    IEnumerator CoSinkThenMaybeBob(GameObject ing)
    {
        if(ing.name == "Chicken")
        {
            ToogleDraggable(ing, false);
            ing.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        yield return new WaitForSeconds(sinkDelay);

        // Sorting theo index trong requiredIngredientsStep1
        int i = requiredIngredientsStep1.IndexOf(ing);
        if (i < 0) i = 0; // guard nhỏ nếu ing không có trong list
        var sr = ing.GetComponent<SpriteRenderer>();
        if(sr == null)
        {
            var raw = ing.transform.GetChild(0);
            sr = raw.GetComponent<SpriteRenderer>();
        }
        if (sr) sr.sortingOrder = underWaterSortingOrder - i;

        if (gasKnob && gasKnob.isOn) StartBobbing(ing);

        if (ing && ing.name == "Chicken")
        {
            allowBobbing = true;          // re-enable bob cho step 2
            StartBobbing(ing);            // đảm bảo gà rung
            if (frothGO) { frothGO.SetActive(true); StartBobbing(frothGO); }
            StartCoroutine(CoCookStep2Chicken(ing)); // nấu 3s, đổi màu
            yield break; // step 2 không cần TryStartCookingStep1
        }


        TryStartCookingStep1(); // đủ + có lửa thì nấu
    }

    // ---------- Gas ON (một lần) ----------
    void OnGasTurnOn()
    {
        allowBobbing = true;
        foreach (var ing in currentIngredientsStep1) StartBobbing(ing);
        TryStartCookingStep1();
    }

    // ---------- Bob control ----------
    void StartBobbing(GameObject go)
    {
        if (!go || _bobCo.ContainsKey(go) || !allowBobbing) return;
        var co = StartCoroutine(CoBobWhileAllowed(go.transform));
        _bobCo[go] = co;
    }

    IEnumerator CoBobWhileAllowed(Transform t)
    {
        var start = t.localPosition;
        float phase = Random.value * Mathf.PI * 2f;
        while (t && allowBobbing)
        {
            float y = Mathf.Sin((Time.time + phase) * bobSpeed) * bobAmplitude;
            t.localPosition = new Vector3(start.x, start.y + y, start.z);
            yield return null;
        }
        if (t) t.localPosition = start;
        if (t) _bobCo.Remove(t.gameObject);
    }

    // ---------- Start cooking step 1 ----------
    void TryStartCookingStep1()
    {
        if (IsStep1Completed || isCookingStep1Running) return;
        if (!gasKnob || !gasKnob.isOn) return; // phải bật lửa
        if (requiredIngredientsStep1.Count == 0) return;

        if (currentIngredientsStep1.Count == requiredIngredientsStep1.Count)
            StartCoroutine(CoCookStep1());
    }

    IEnumerator CoCookStep1()
    {
        isCookingStep1Running = true;

        var pepper = currentIngredientsStep1.Find(g => g && g.name == "Pepper");
        var lemonLeaf = currentIngredientsStep1.Find(g => g && g.name == "LemonLeaf");
        var beef = currentIngredientsStep1.Find(g => g && g.name == "Beef");

        // Water & Broth
        waterGO?.GetComponent<Animator>()?.SetTrigger("Disappear");
        if (brothGO) brothGO.SetActive(true);

        const float cookDur = 3f;
        float t = 0f;

        // Fade Pepper & LemonLeaf
        var srPep = pepper ? pepper.GetComponent<SpriteRenderer>() : null;
        var srLem = lemonLeaf ? lemonLeaf.GetComponent<SpriteRenderer>() : null;
        float a0P = srPep ? srPep.color.a : 1f;
        float a0L = srLem ? srLem.color.a : 1f;

        // Beef: Raw/Cooked overlay
        SpriteRenderer srRawBeef = null, srCookedBeef = null;
        Transform rawT = beef ? beef.transform.Find("Raw") : null;
        Transform cooked = rawT ? rawT.Find("Cooked") : null;

        if (rawT) srRawBeef = rawT.GetComponent<SpriteRenderer>();
        if (cooked) srCookedBeef = cooked.GetComponent<SpriteRenderer>();

        // Bật overlay Cooked (alpha 0) để blend trong quá trình nấu
        if (cooked)
        {
            cooked.gameObject.SetActive(true);
            if (srCookedBeef)
            {
                var c = srCookedBeef.color; c.a = 0f; srCookedBeef.color = c;
            }
        }

        // Blend: Raw ↓0, Cooked ↑1
        while (t < cookDur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / cookDur);

            if (srPep) { var c = srPep.color; c.a = Mathf.Lerp(a0P, 0f, k); srPep.color = c; }
            if (srLem) { var c = srLem.color; c.a = Mathf.Lerp(a0L, 0f, k); srLem.color = c; }

            if (srRawBeef) { var c = srRawBeef.color; c.a = Mathf.Lerp(1f, 0f, k); srRawBeef.color = c; }
            if (srCookedBeef) { var c = srCookedBeef.color; c.a = Mathf.Lerp(0f, 1f, k); srCookedBeef.color = c; }

            yield return null;
        }

        // Kết thúc: gán sprite cooked vào Raw, set alpha Raw = 1, tắt overlay Cooked
        if (srRawBeef && srCookedBeef)
        {
            srRawBeef.sprite = srCookedBeef.sprite;
            srRawBeef.transform.localScale = Vector3.one;   // reset scale nếu Cooked có scale khác
            // Raw dùng sprite "cooked" từ nay
            var c = srRawBeef.color; c.a = 1f; srRawBeef.color = c;

            if (cooked)
            {
                cooked.gameObject.SetActive(false);         // Cooked chỉ dùng để blend
                                                            // reset alpha Cooked về 0 cho lần sau (nếu có)
                var cc = srCookedBeef.color; cc.a = 0f; srCookedBeef.color = cc;
            }
        }

        if (lemonLeaf) lemonLeaf.SetActive(false);          // tránh che collider Beef

        var beefComp = beef ? beef.GetComponent<Beef>() : null;
        if (beefComp != null) beefComp.canCut = true;       // cho phép nhấc/cắt

        // Dừng rung sau khi nấu xong để nhấc thịt mượt
        allowBobbing = false;
        _bobCo.Clear();

        IsStep1Completed = true;
        ToogleDraggable(beef, IsStep1Completed);
        isCookingStep1Running = false;
    }

    private void ToogleDraggable(GameObject obj, bool canDrag)
    {
        var dragComp = obj.GetComponent<DraggableItem>();
        if(dragComp) dragComp.draggable = canDrag;
    }
    public void UnpackPotContents(bool keepWorldPosition = true)
    {
        if (!potContentRoot) return;
        var tmp = new List<Transform>(potContentRoot.childCount);
        var currentStep = transform.Find("BrothBoil");
        foreach (Transform c in potContentRoot) tmp.Add(c);
        for (int i = 0; i < tmp.Count; i++)
            if (tmp[i]) tmp[i].SetParent(currentStep, keepWorldPosition);
    }
    IEnumerator CoCookStep2Chicken(GameObject chicken)
    {
        var sr = chicken ? chicken.GetComponent<SpriteRenderer>() : null;
        if (!sr && chicken && chicken.transform.childCount > 0)
            sr = chicken.transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (sr)
        {
            var start = Color.white;                   // 255,255,255,255
            var target = (Color)new Color32(192, 158, 158, 255); // #C09E9E
            const float dur = 3f;
            float t = 0f;
            sr.color = start;

            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                sr.color = Color.Lerp(start, target, k);
                yield return null;
            }
            sr.color = target; // chốt màu đích
        }

        yield return new WaitForSeconds(3f);
        allowBobbing = false;
        _bobCo.Clear();
        ToogleDraggable(chicken, true);

    }

}
