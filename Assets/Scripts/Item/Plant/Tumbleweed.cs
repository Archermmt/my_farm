public class Tumbleweed : Plant {
    public override void DestroyItem(FieldGrid grid) {
        base.DestroyItem(grid);
        Destroy(transform.parent.gameObject);
    }
}
