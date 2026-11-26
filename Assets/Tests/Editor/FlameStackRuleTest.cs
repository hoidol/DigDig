using NUnit.Framework;

// (2026-11-26) 화상 최대 3중첩: 3개 미만이면 추가, 다 찼으면 가장 약한 중첩보다 셀 때만 교체
public class FlameStackRuleTest
{
    [Test]
    public void Stack_Example()
    {
        CollectionAssert.AreEquivalent(new[] { 2f, 3f, 4f }, FlameEffect.SimulateStacks(new[] { 2f, 3f, 4f }), "3초까지 2+3+4");
        CollectionAssert.AreEquivalent(new[] { 2f, 3f, 4f }, FlameEffect.SimulateStacks(new[] { 2f, 3f, 4f, 1f }), "1은 무시");
        CollectionAssert.AreEquivalent(new[] { 5f, 3f, 4f }, FlameEffect.SimulateStacks(new[] { 2f, 3f, 4f, 1f, 5f }), "5는 가장 약한 2와 교체");
        CollectionAssert.AreEquivalent(new[] { 2f, 3f, 4f }, FlameEffect.SimulateStacks(new[] { 2f, 3f, 4f, 2f }), "같은 피해는 무시");
    }
}
