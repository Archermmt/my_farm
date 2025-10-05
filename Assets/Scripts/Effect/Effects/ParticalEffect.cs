public class ParticalEffect : Effect {
    public override void StartEffect(EffectMeta data) {
        gameObject.SetActive(true);
    }

    public override void EndEffect(EffectMeta data) {
        gameObject.SetActive(false);
    }
}
