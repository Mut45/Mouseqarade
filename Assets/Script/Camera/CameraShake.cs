using UnityEngine;

/// <summary>
/// 极简相机震动脚本（仅核心功能，可直接调用）
/// 挂载在MainCamera上，通过ShakeCamera()方法触发震动
/// </summary>
public class CameraShake : MonoBehaviour
{
    // 私有变量：震动状态+初始位置
    private bool isShaking; // 是否正在震动
    private Vector3 originalPosition; // 相机初始位置
    private float shakeTimer; // 震动剩余时长
    private float shakeMagnitude; // 震动幅度（偏移量）
    private float shakeFrequency; // 震动频率（越快越抖）

    private void Start()
    {
        // 初始化：记录相机初始位置
        originalPosition = transform.localPosition;
        isShaking = false;
    }

    private void Update()
    {
        // 仅在震动中执行逻辑
        if (isShaking && shakeTimer > 0)
        {
            // 用PerlinNoise生成平滑的随机偏移（避免生硬抖动）
            float offsetX = Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2 - 1;
            float offsetY = Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2 - 1;
            // 计算震动偏移（乘以幅度控制强度）
            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0) * shakeMagnitude;

            // 应用震动偏移
            transform.localPosition = originalPosition + shakeOffset;

            // 递减计时
            shakeTimer -= Time.deltaTime;
        }
        else if (isShaking)
        {
            // 震动结束：恢复初始位置 + 重置状态
            isShaking = false;
            transform.localPosition = originalPosition;
        }
    }

    /// <summary>
    /// 公开调用方法：触发相机震动（核心接口）
    /// </summary>
    /// <param name="duration">震动时长（秒，默认0.5秒）</param>
    /// <param name="magnitude">震动幅度（默认0.1，值越大越抖）</param>
    /// <param name="frequency">震动频率（默认20，值越大越快）</param>
    public void ShakeCamera(float duration = 0.5f, float magnitude = 0.1f, float frequency = 20f)
    {
        // 赋值震动参数
        shakeTimer = duration;
        shakeMagnitude = magnitude;
        shakeFrequency = frequency;
        // 标记开始震动
        isShaking = true;
    }
}