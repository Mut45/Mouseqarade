using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuffButtonHandler : MonoBehaviour
{
    [Header("核心配置（仅需配这1个）")]
    [Tooltip("当前Buff预制体对应的BuffId")]
    public BuffId targetBuffId; // 只需要指定BuffId，无需手动拖BuffManager

    [Header("按钮配置")]
    public Button buffSelectButton;

    [Header("可选：额外触发事件")]
    public UnityEvent onBuffButtonClicked;

    [HideInInspector]
    public GameObject otherBuffPrefab;

    private BuffManager buffManager; // 改为私有，自动查找
    private bool isDestroying = false;

    private void Awake()
    {
        // 1. 自动查找BuffManager单例（原有逻辑）
        buffManager = BuffManager.Instance;
        if (buffManager == null)
        {
            Debug.LogError("场景中未找到BuffManager单例！请确保场景中有BuffManager物体", this);
        }

        // 2. 校验Button组件（原有逻辑）
        if (buffSelectButton == null)
        {
            Debug.LogError("未绑定Button组件！", this);
            return;
        }

        // 绑定点击事件（原有逻辑）
        buffSelectButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (isDestroying) return;
        isDestroying = true;

        try
        {
            // 原有逻辑：调用AcquireBuff
            if (buffManager != null)
            {
                buffManager.AcquireBuff(targetBuffId);
                Debug.Log($"[{gameObject.name}] 激活Buff：{targetBuffId}");
            }

            // 原有逻辑：触发自定义事件、销毁另一个Buff、恢复游戏
            if (onBuffButtonClicked != null) onBuffButtonClicked.Invoke();
            if (otherBuffPrefab != null) Destroy(otherBuffPrefab);
            Destroy(gameObject);
            Time.timeScale = 1f;
        }
        finally
        {
            isDestroying = false;
        }
    }

    private void OnDestroy()
    {
        if (buffSelectButton != null)
        {
            buffSelectButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}