// WaveData/{난이도}.csv 한 행 = startWave부터 다음 행의 startWave 전까지 적용되는 구간 (무한 웨이브)
[System.Serializable]
public class WaveData
{
    public int startWave; // 이 구간이 시작되는 waveIdx
    public DefenceType[] defenceTypes; // 이 구간에서 등장하는 방어 타입 (Random일 때 이 중에서 뽑음)
    public DefenceWaveType[] defenceWaveTypes;// 구간 시작부터 wave마다 순서대로 적용 ((wave - startWave) % 개수), 한 wave 안에서는 모두 같은 방어 타입

    // wave의 방어 타입. Random이면 defenceTypes 중 하나 (호출할 때마다 새로 뽑으므로 wave 시작 시 한 번만 호출)
    public DefenceType GetDefenceType(int waveIdx)
    {
        if (defenceWaveTypes == null || defenceWaveTypes.Length == 0)
            return DefenceType.Normal;

        DefenceWaveType waveType = defenceWaveTypes[(waveIdx - startWave) % defenceWaveTypes.Length];
        if (waveType == DefenceWaveType.Random)
        {
            if (defenceTypes == null || defenceTypes.Length == 0)
                return DefenceType.Normal;
            return defenceTypes[UnityEngine.Random.Range(0, defenceTypes.Length)];
        }

        // DefenceWaveType은 Random 다음부터 DefenceType과 같은 이름
        return (DefenceType)System.Enum.Parse(typeof(DefenceType), waveType.ToString());
    }
}
