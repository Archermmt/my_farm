public class ParticalEffect : Effect {
    public override void StartEffect(EffectMeta effect) {
        gameObject.SetActive(true);
    }

    public override void EndEffect(EffectMeta effect) {
        gameObject.SetActive(false);
    }
}
