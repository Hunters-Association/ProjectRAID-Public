public enum ZoneDangerLevel
{
    Safe,
    Warning,
    Dangerous
}

[System.Serializable]
public class AreaInfo
{
    public string regionName;       // 예: Academy
    public string subregionName;    // 예: Town square
    public ZoneDangerLevel dangerLevel;
}
