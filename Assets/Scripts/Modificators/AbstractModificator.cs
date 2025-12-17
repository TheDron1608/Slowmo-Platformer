using UnityEngine;

public abstract class AbstractModificator : MonoBehaviour
{
    public ModificatorIcon IconInstance;
    public ModificatorCard CardInstance;

    public bool GetEqualType(AbstractModificator other)
    {
        return GetType() == other.GetType();
    }
}