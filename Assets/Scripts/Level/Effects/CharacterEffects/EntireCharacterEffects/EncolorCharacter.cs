using UnityEngine;

public class EncolorCharacter : AbstractCharacterEffectWithSender, IEntireCharacterEffect
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (sender.TryGetComponent(out Renderer senderRenderer))
        {
            foreach (CharacterPart part in AffectedCharacter.CharacterPartsManager.CharacterParts)
            {
                if (part.TryGetComponent(out DynamicMaterial dynamicMaterial))
                {
                    dynamicMaterial.DefaultMaterial = senderRenderer.sharedMaterial;
                    dynamicMaterial.AllowChangeMaterial = false;
                }
            }
        }
    }
}