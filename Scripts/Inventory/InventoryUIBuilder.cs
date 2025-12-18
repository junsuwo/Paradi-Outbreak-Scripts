#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class InventoryUIBuilder
{
    [MenuItem("Tools/Build Inventory UI (Full)")]
    public static void Build()
    {
        // Canvas + EventSystem
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            var cvGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cvGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = cvGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
        }
        if (!Object.FindFirstObjectByType<EventSystem>())
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Panel
        var panel = New("InventoryPanel", canvas.transform, typeof(Image), typeof(InventoryUI));
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;
        panel.SetActive(false);
        panel.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.07f, 0.92f);

        var ui = panel.GetComponent<InventoryUI>();

        // Header
        var header = NewTMP("Header", panel.transform, "Inventory", 64, TextAlignmentOptions.Center, new Color32(180, 30, 30, 255));
        var headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = headerRT.anchorMax = new Vector2(0.5f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = new Vector2(0, -40);
        headerRT.sizeDelta = new Vector2(800, 80);

        // Left
        var left = New("LeftSection", panel.transform);
        var leftRT = left.GetComponent<RectTransform>();
        leftRT.anchorMin = leftRT.anchorMax = new Vector2(0f, 0.5f);
        leftRT.pivot = new Vector2(0f, 0.5f);
        leftRT.anchoredPosition = new Vector2(100, 0);
        leftRT.sizeDelta = new Vector2(820, 850);

        // SlotGrid
        var grid = New("SlotGrid", left.transform, typeof(GridLayoutGroup));
        var gridRT = grid.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0, 0); gridRT.anchorMax = new Vector2(0, 1);
        gridRT.pivot = new Vector2(0, 0.5f);
        gridRT.sizeDelta = new Vector2(520, 850);
        var gl = grid.GetComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(92, 92);
        gl.spacing = new Vector2(10, 10);
        gl.childAlignment = TextAnchor.UpperLeft;
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 8;

        // ItemDetail
        var detail = New("ItemDetail", left.transform);
        var detailRT = detail.GetComponent<RectTransform>();
        detailRT.anchorMin = Vector2.zero; detailRT.anchorMax = Vector2.one;
        detailRT.offsetMin = new Vector2(560, 0); detailRT.offsetMax = new Vector2(0, 0);

        var nameTxt = NewTMP("ItemName", detail.transform, "아이템 이름", 36, TextAlignmentOptions.Left, new Color32(200, 40, 40, 255));
        var nameRT = nameTxt.GetComponent<RectTransform>();
        nameRT.anchorMin = nameRT.anchorMax = new Vector2(0, 1); nameRT.pivot = new Vector2(0, 1);
        nameRT.sizeDelta = new Vector2(0, 50);

        var descTxt = NewTMP("ItemDescription", detail.transform, "아이템 설명...", 24, TextAlignmentOptions.TopLeft, Color.white);
        var descRT = descTxt.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 0); descRT.anchorMax = new Vector2(1, 1);
        descRT.offsetMin = new Vector2(0, 80); descRT.offsetMax = new Vector2(0, -60);

        var useBtn = NewButton("UseButton", detail.transform, "사용");
        var useRT = useBtn.GetComponent<RectTransform>();
        useRT.anchorMin = useRT.anchorMax = new Vector2(1, 0); useRT.pivot = new Vector2(1, 0);
        useRT.sizeDelta = new Vector2(160, 56);

        // Right
        var right = New("RightSection", panel.transform);
        var rightRT = right.GetComponent<RectTransform>();
        rightRT.anchorMin = rightRT.anchorMax = new Vector2(1, 0.5f);
        rightRT.pivot = new Vector2(1, 0.5f);
        rightRT.anchoredPosition = new Vector2(-80, 0);
        rightRT.sizeDelta = new Vector2(900, 850);

        var profile = New("ProfileImage", right.transform, typeof(Image));
        var pRT = profile.GetComponent<RectTransform>();
        pRT.anchorMin = pRT.anchorMax = new Vector2(1, 1); pRT.pivot = new Vector2(1, 1);
        pRT.anchoredPosition = new Vector2(-40, -40);
        pRT.sizeDelta = new Vector2(160, 160);

        var statList = New("StatList", right.transform, typeof(VerticalLayoutGroup));
        var sRT = statList.GetComponent<RectTransform>();
        sRT.anchorMin = sRT.anchorMax = new Vector2(0, 1); sRT.pivot = new Vector2(0, 1);
        sRT.anchoredPosition = new Vector2(40, -40);
        sRT.sizeDelta = new Vector2(480, 420);
        var vlg = statList.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft; vlg.spacing = 8;

        string[] statNames = {
            "공격력","공격 데미지","방어력","피해 감소율",
            "체력","생명력","공격속도","공격 빈도",
            "이동속도","이동 속도","체력 회복력","초당 체력 회복량"
        };
        foreach (var n in statNames) NewTMP(n, statList.transform, $"{n}: 0", 24, TextAlignmentOptions.Left, Color.white);

        var essence = New("EssenceSlots", right.transform, typeof(GridLayoutGroup));
        var eRT = essence.GetComponent<RectTransform>();
        eRT.anchorMin = eRT.anchorMax = new Vector2(0, 0.5f); eRT.pivot = new Vector2(0, 0.5f);
        eRT.anchoredPosition = new Vector2(40, -40);
        eRT.sizeDelta = new Vector2(480, 120);
        var eGrid = essence.GetComponent<GridLayoutGroup>();
        eGrid.cellSize = new Vector2(100, 100); eGrid.spacing = new Vector2(12, 0);
        eGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; eGrid.constraintCount = 4;

        var effect = NewTMP("StatEffectPanel", right.transform, "스텟, 정수 효과 & 추가 증가량", 24, TextAlignmentOptions.TopLeft, Color.white);
        var effRT = effect.GetComponent<RectTransform>();
        effRT.anchorMin = new Vector2(0.4f, 0f); effRT.anchorMax = new Vector2(1f, 0.4f);
        effRT.pivot = new Vector2(1, 0); effRT.anchoredPosition = new Vector2(-20, 20);

        // ===== 템플릿(슬롯) 생성 =====
        var slotTemplate = MakeSlotTemplate("SlotTemplate", panel.transform);
        var essenceTemplate = MakeEssenceTemplate("EssenceTemplate", panel.transform);

        // ===== Inventory 컴포넌트 배치/연결 =====
        var sys = GameObject.Find("System") ?? new GameObject("System");
        var inv = sys.GetComponent<Inventory>() ?? sys.AddComponent<Inventory>();
        inv.inventoryPanel = panel;
        inv.capacity = 40;

        ui.gridParent = grid.transform;
        ui.slotTemplate = slotTemplate;
        ui.itemName = nameTxt;
        ui.itemDesc = descTxt;
        ui.useButton = useBtn.GetComponent<Button>();
        ui.profileImage = profile.GetComponent<Image>();
        ui.statList = statList.transform;
        ui.essenceParent = essence.transform;
        ui.essenceTemplate = essenceTemplate;

        ui.extraEffectText = effect;

        inv.inventoryUI = ui;

        // PlayerStats 연결 확인
        var player = Object.FindFirstObjectByType<PlayerStats>();
        if (player)
        {
            player.Recalculate();
        }
        else
        {
            Debug.LogWarning("[InventoryUIBuilder] PlayerStats를 찾지 못했습니다. 실제 게임에서는 StageManager에서 로컬 플레이어가 생성되면 자동 연결됩니다.");
        }


        Selection.activeObject = panel;
        EditorGUIUtility.PingObject(panel);
        Debug.Log("<color=lime>[InventoryUIBuilder]</color> Full UI 생성 완료!  배경 스프라이트만 Panel Image에 드래그해서 바꿔주세요.");
    }

    // ---------- helpers ----------
    static GameObject New(string name, Transform parent, params System.Type[] extra)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        foreach (var t in extra) go.AddComponent(t);
        return go;
    }

    static TMP_Text NewTMP(string name, Transform parent, string text, float size, TextAlignmentOptions align, Color color)
    {
        var go = New(name, parent, typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color; tmp.raycastTarget = false;
        return tmp;
    }

    static GameObject NewButton(string name, Transform parent, string label)
    {
        var go = New(name, parent, typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        var txt = NewTMP("Label", go.transform, label, 28, TextAlignmentOptions.Center, Color.white);
        var rt = txt.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    static InventorySlotUI MakeSlotTemplate(string name, Transform parent)
    {
        var go = New(name, parent, typeof(CanvasRenderer));
        var bg = go.AddComponent<Image>(); bg.color = new Color(0, 0, 0, 0.4f);

        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(92, 92);

        var icon = New("Icon", go.transform, typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        var irt = icon.GetComponent<RectTransform>(); irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f); irt.sizeDelta = new Vector2(80, 80);
        icon.preserveAspect = true;

        var count = NewTMP("Count", go.transform, "99", 20, TextAlignmentOptions.BottomRight, Color.white);
        var crt = count.GetComponent<RectTransform>(); crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = new Vector2(6, 6); crt.offsetMax = new Vector2(-6, -6);

        var hl = New("Highlight", go.transform, typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        hl.color = new Color(0.3f, 0.7f, 1f, 0.4f); hl.enabled = false;
        var hrt = hl.GetComponent<RectTransform>(); hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one; hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;

        var ui = go.AddComponent<InventorySlotUI>();
        ui.click = btn; ui.icon = icon; ui.countText = count; ui.highlight = hl;

        go.SetActive(false); // 템플릿이라 숨김
        return ui;
    }

    static EssenceSlotUI MakeEssenceTemplate(string name, Transform parent)
    {
        var go = New(name, parent, typeof(CanvasRenderer));
        var bg = go.AddComponent<Image>(); bg.color = new Color(0, 0, 0, 0.35f);
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(100, 100);

        var icon = New("Icon", go.transform, typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        var irt = icon.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.sizeDelta = new Vector2(86, 86);
        icon.preserveAspect = true;
        icon.enabled = false;

        var ui = go.AddComponent<EssenceSlotUI>();
        ui.icon = icon; // ✅ 반드시 추가
                        // ui.click = btn; // 클릭 필요 시 나중에 추가

        go.SetActive(false);
        return ui;
    }

}
#endif
