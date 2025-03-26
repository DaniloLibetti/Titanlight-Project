using UnityEngine;

public abstract class CharacterStatsModifierSO : ScriptableObject
{
    public abstract void AffectCharacter(GameObject character, float val);
}
