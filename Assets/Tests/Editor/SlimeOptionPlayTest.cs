using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 유닛 설정 버튼 플레이 테스트 (CLAUDE.md "유닛 설정 버튼")
// - 슬라임 종류에 맞는 설정 버튼만 노출 (타겟 방식 / 버프 범위 / 회전 방향)
// - 버프 범위 십자 ↔ 대각, 회전체 시계 ↔ 반시계가 실제로 반영되는지
public class SlimeOptionPlayTest : InGamePlayTestBase
{
    const string NORMAL_SLIME = "Ice";
    const string CROSS_BUFF_SLIME = "Breeze";
    const string ORBIT_SLIME = "Orbit";
    const string HAIL_SLIME = "Hail";

    static readonly Vector2Int[] CROSS = { new(0, 1), new(0, -1), new(-1, 0), new(1, 0) };
    static readonly Vector2Int[] DIAGONAL = { new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };

    [UnityTest]
    public IEnumerator SettingPanel_ShowsOnlyAvailableButtons()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return SettingPanelBody();
    }

    IEnumerator SettingPanelBody()
    {
        AllySettingPanel panel = Object.FindFirstObjectByType<AllySettingPanel>(FindObjectsInactive.Include);
        Assert.IsNotNull(panel, "AllySettingPanel 없음");
        Assert.IsNotNull(panel.setBuffRangeButton, "씬에 버프 범위 버튼이 연결되지 않음");
        Assert.IsNotNull(panel.setRotationDirectionButton, "씬에 회전 방향 버튼이 연결되지 않음");

        Tile t0 = GetFreeTile();
        Slime normal = SpawnSlime(NORMAL_SLIME, t0);
        Tile t1 = GetFreeTile(t0);
        Slime buff = SpawnSlime(CROSS_BUFF_SLIME, t1);
        Tile t2 = GetFreeTile(t0, t1);
        Slime orbit = SpawnSlime(ORBIT_SLIME, t2);
        Tile t3 = GetFreeTile(t0, t1, t2);
        Slime hail = SpawnSlime(HAIL_SLIME, t3);
        yield return WaitFrames(2);

        AssertButtons(panel, normal, target: true, buffRange: false, rotation: false);
        AssertButtons(panel, buff, target: true, buffRange: true, rotation: false);
        AssertButtons(panel, orbit, target: false, buffRange: false, rotation: true);
        AssertButtons(panel, hail, target: false, buffRange: false, rotation: true);
        AssertButtons(panel, KingSlime.Instance, target: true, buffRange: false, rotation: false);

        // 버튼 클릭 → 값 순환
        panel.SetSlime(buff);
        panel.setBuffRangeButton.OnClickedButton();
        Assert.AreEqual(BuffRangeShape.Diagonal, ((IBuffRangeUnit)buff).BuffRangeShape);
        panel.setBuffRangeButton.OnClickedButton();
        Assert.AreEqual(BuffRangeShape.Cross, ((IBuffRangeUnit)buff).BuffRangeShape);

        panel.SetSlime(hail);
        RotationDirection before = ((IRotationDirectionUnit)hail).RotationDirection;
        panel.setRotationDirectionButton.OnClickedButton();
        Assert.AreNotEqual(before, ((IRotationDirectionUnit)hail).RotationDirection);

        panel.SetSlime(null);
        AssertNoErrors();
    }

    static void AssertButtons(AllySettingPanel panel, IAllyUnit unit, bool target, bool buffRange, bool rotation)
    {
        panel.SetSlime(unit);
        string name = unit.Key;
        Assert.AreEqual(target, panel.setFindTargetButton.gameObject.activeSelf, $"{name}: 타겟 방식 버튼");
        Assert.AreEqual(buffRange, panel.setBuffRangeButton.gameObject.activeSelf, $"{name}: 버프 범위 버튼");
        Assert.AreEqual(rotation, panel.setRotationDirectionButton.gameObject.activeSelf, $"{name}: 회전 방향 버튼");
    }

    // 십자 → 대각으로 바꾸면 버프 대상이 십자 칸 슬라임에서 대각 칸 슬라임으로 바뀐다 (같은 TileGroup 안)
    [UnityTest]
    public IEnumerator BuffRange_CrossToDiagonal()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return BuffRangeBody();
    }

    IEnumerator BuffRangeBody()
    {
        // 같은 그룹 안에 십자 칸 하나, 대각 칸 하나가 비어 있는 자리
        Tile center = null, cross = null, diagonal = null;
        foreach (Tile t in TileManager.Instance.tiles.Where(t => t.IsEmpty))
        {
            cross = FindNeighbor(t, CROSS);
            diagonal = FindNeighbor(t, DIAGONAL);
            if (cross != null && diagonal != null) { center = t; break; }
        }
        Assert.IsNotNull(center, "테스트할 자리 없음 (같은 그룹 안 십자·대각 빈 칸)");

        Slime crossTarget = SpawnSlime(NORMAL_SLIME, cross);
        Slime diagonalTarget = SpawnSlime(NORMAL_SLIME, diagonal);
        yield return WaitFrames(3);
        float crossBase = crossTarget.AttackSpeed;
        float diagonalBase = diagonalTarget.AttackSpeed;

        CrossBuffSlime buffer = (CrossBuffSlime)SpawnSlime(CROSS_BUFF_SLIME, center);
        yield return WaitFrames(3);
        Assert.AreEqual(BuffRangeShape.Cross, buffer.BuffRangeShape, "기본은 십자");
        Assert.Greater(crossTarget.AttackSpeed, crossBase, "십자: 십자 칸 슬라임이 버프를 못 받음");
        Assert.AreEqual(diagonalBase, diagonalTarget.AttackSpeed, 0.0001f, "십자: 대각 칸 슬라임이 버프를 받음");

        buffer.SetBuffRangeShape(BuffRangeShape.Diagonal);
        yield return WaitFrames(3);
        Assert.AreEqual(crossBase, crossTarget.AttackSpeed, 0.0001f, "대각: 십자 칸 버프가 남음");
        Assert.Greater(diagonalTarget.AttackSpeed, diagonalBase, "대각: 대각 칸 슬라임이 버프를 못 받음");

        SlimeFieldController.Instance.RemoveFieldSlime(buffer);
        yield return WaitFrames(3);
        Assert.AreEqual(diagonalBase, diagonalTarget.AttackSpeed, 0.0001f, "버프 슬라임이 사라졌는데 버프가 남음");
        AssertNoErrors();
    }

    static Tile FindNeighbor(Tile center, Vector2Int[] offsets)
    {
        return offsets.Select(o => TileManager.Instance.GetTile(center.index + o))
            .FirstOrDefault(n => n != null && n.IsEmpty && n.tileGroup == center.tileGroup);
    }

    // 회전체: 기본 반시계(z 각도 증가), 시계로 바꾸면 z 각도 감소
    [UnityTest]
    public IEnumerator Orbit_RotationDirection()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return OrbitBody();
    }

    IEnumerator OrbitBody()
    {
        OrbitSlime orbit = (OrbitSlime)SpawnSlime(ORBIT_SLIME, GetFreeTile());
        yield return WaitFrames(2);
        Assert.AreEqual(OrbitMachine.DEFAULT_ROTATION_DIRECTION, orbit.RotationDirection);

        float ccw = 0f;
        yield return MeasureRotation(orbit.orbitMachine.transform, d => ccw = d);
        Assert.Greater(ccw, 0f, $"반시계인데 z 각도가 늘지 않음 (rotationSpeed {orbit.orbitMachine.rotationSpeed}, active {orbit.orbitMachine.isActiveAndEnabled}, timeScale {Time.timeScale})");

        orbit.SetRotationDirection(RotationDirection.Clockwise);
        float cw = 0f;
        yield return MeasureRotation(orbit.orbitMachine.transform, d => cw = d);
        Assert.Less(cw, 0f, "시계인데 z 각도가 줄지 않음");
        AssertNoErrors();
    }

    static IEnumerator MeasureRotation(Transform t, System.Action<float> result)
    {
        float total = 0f;
        float prev = t.eulerAngles.z;
        double end = UnityEditor.EditorApplication.timeSinceStartup + 0.5;
        while (UnityEditor.EditorApplication.timeSinceStartup < end)
        {
            yield return WaitFrames(1);
            float now = t.eulerAngles.z;
            total += Mathf.DeltaAngle(prev, now);
            prev = now;
        }
        result(total);
    }
}
