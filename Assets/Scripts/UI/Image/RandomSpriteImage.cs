using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RandomSpriteImage : MonoBehaviour
{
    public List<Sprite> RandomSprites = new();

    private void Awake()
    {
        GetComponent<Image>().sprite = NumberMath.PickRandomItem(RandomSprites);
    }
}