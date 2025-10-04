public class ParticalEffect : Effect {
    public override void StartEffect(EffectData data) {
        gameObject.SetActive(true);
    }

    public override void EndEffect(EffectData data) {
        gameObject.SetActive(false);
    }
}
