using UnityEngine.Serialization;

[System.Serializable]
public class DecalType
{
    [FormerlySerializedAs("spellID")]
    public SpellDecalType decalType;
    public SpellDecalDissolve decalPrefab;
    public int pooledCount = 8;
}
