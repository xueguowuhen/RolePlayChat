/// <summary>
/// SettingData 用于存储用户设置，例如音量、通知开关等。可按需扩展字段。
/// </summary>
public class SettingData
{
    /// <summary>
    /// 背景音乐音量，范围 [0.0f, 1.0f]，默认 1.0
    /// </summary>
    public float volume { get; set; } = 1.0f;

    /// <summary>
    /// 是否启用通知，默认 true
    /// </summary>
    public bool notificationsEnabled { get; set; } = true;

    // TODO: 根据项目需求，可添加更多配置，例如：
    // public bool darkModeEnabled { get; set; } = false;
    // public Dictionary<string, string> userPrefs { get; set; } = new Dictionary<string, string>();
}
public class MessageSettingData
{
    public string SystemPrompt { get; set; } //系统最高提示词
    public string SystemWorldPrompt { get; set; } //系统世界观提示词
    public string SystemYourRolePrompt { get; set; } //系统角色提示词

    public string SystemMyRoleName { get; set; } //系统角色名称

}