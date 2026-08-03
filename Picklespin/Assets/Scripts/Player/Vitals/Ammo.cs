using UnityEngine;

public class Ammo : MonoBehaviour
{
    public static Ammo instance { get; private set; }
    public int ammo;
    public int maxAmmo;

    // one number for the crit boost, the bar's pulse and Umbral's stamina floor
    public const float LowMagickaThreshold = 0.2f;

    public float Fraction => maxAmmo > 0 ? (float)ammo / maxAmmo : 0f;
    public bool IsLow => Fraction < LowMagickaThreshold;

    // what the bar draws: the whole points plus whatever a continuous drain has taken
    // since the last one. The bar polls this, so a step and a drain on the same pool
    // in the same frame can never arrive in the wrong order.
    public float DisplayFraction => maxAmmo > 0
        ? Mathf.Clamp01((ammo - drainRemainder - staminaRemainder) / maxAmmo)
        : 0f;

    // sitting *on* the line already counts as spent: stamina cannot push past it, so
    // anything short of <= would leave the last point buying an endless sprint
    public int StaminaFloorPoints => Mathf.CeilToInt(maxAmmo * LowMagickaThreshold);
    public bool AtStaminaFloor => Fraction <= LowMagickaThreshold;

    private BarLightsAnimation barLightsAnimation;
    private AmmoDisplay ammoDisplay;
    private float drainRemainder;
    private float staminaRemainder;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Start()
    {
        barLightsAnimation = BarLightsAnimation.instance;
        ammoDisplay = AmmoDisplay.instance;
    }

    public void GiveManaToPlayer(int howMuchManaIGive, bool isSilent = false)
    {
        ammo = Mathf.Clamp(ammo + howMuchManaIGive, 0, maxAmmo);
        bool gotMaxxed = ammo == maxAmmo;
        // a gain eases in; a loss snaps, the way spending magicka does, so the BarEase
        // shadow has a chunk to show. Damage arrives here when this bar is the health pool.
        ammoDisplay.Refresh(howMuchManaIGive >= 0);

        if (!isSilent)
        {
            barLightsAnimation.PlaySelectedBarAnimation(2, howMuchManaIGive, gotMaxxed);
        }

        MagickaChanged();
    }

    // when magicka is the health pool, every write has to re-run the low-health
    // check - call it after writing `ammo` directly too, as Attack and Dash do
    public void MagickaChanged()
    {
        if (PlayerClasses.MagickaIsHealth && PlayerHP.Instance) PlayerHP.Instance.RefreshLowHealthState();
    }

    // the fractional part is carried between frames, so the bar falls at a constant rate
    public void DrainMana(float amount)
    {
        if (amount <= 0)
        {
            return;
        }

        drainRemainder += amount;
        int wholePoints = Mathf.FloorToInt(drainRemainder);

        if (wholePoints > 0)
        {
            drainRemainder -= wholePoints;
            ammo = Mathf.Max(0, ammo - wholePoints);
        }

        if (ammo == 0)
        {
            drainRemainder = 0;
        }

        ammoDisplay.SetContinuousValue(ammo - drainRemainder, maxAmmo);
        MagickaChanged();
    }

    // its own remainder, so an angel heal draining at the same time cannot swallow it
    public void SpendAsStamina(float cost)
    {
        if (cost <= 0f || AtStaminaFloor) return;

        staminaRemainder += cost;
        int wholePoints = Mathf.FloorToInt(staminaRemainder);
        if (wholePoints <= 0) return;

        staminaRemainder -= wholePoints;
        ammo = Mathf.Max(StaminaFloorPoints, ammo - wholePoints);
        MagickaChanged();
    }

    // sprinting stopped: the pending fraction would otherwise hold the bar a point low
    public void StopStaminaSpend() => staminaRemainder = 0f;

    public void StopDraining()
    {
        drainRemainder = 0;
        ammoDisplay.Refresh(false);
    }

    // the extra headroom is granted filled, so the bar grows instead of reading as empty
    public void MultiplyMaxMana(float factor)
    {
        int gained = Mathf.Max(1, Mathf.RoundToInt(maxAmmo * factor)) - maxAmmo;
        maxAmmo += gained;
        GiveManaToPlayer(gained, true);
    }
}
