using UnityEngine;

[CreateAssetMenu]
public class CharacterStatHealthModifierSO : CharacterStatsModifierSO
{
   public override void AffectCharacter(GameObject character, float val)
    {
        Health health = character.GetComponent<Health>();
        if (health != null)
        {
            health.AddHealth((int)val);
        }
    }
}
