using UnityEngine;

[CreateAssetMenu(fileName = "NewFormationSettings", menuName = "Formations/FormationSettings")]
public class FormationSettings : ScriptableObject
{
    public bool IsFirstIndexCenter;

    public int MaxUnits;

    public float XOffset;
    public float YOffset;
    public float ZOffset;
}
