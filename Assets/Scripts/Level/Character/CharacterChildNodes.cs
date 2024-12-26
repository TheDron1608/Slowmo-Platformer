using UnityEngine;

public class CharacterChildNodes : MonoBehaviour
{
    const string CENTER_NODE_NAME = "Center";
    const string CHARACTER_PARTS_NAME = "CharacterParts";

    private GameObject _center;
    private GameObject _characterParts;

    public GameObject Center
    {
        get => _center;
        private set => _center = value;
    }

    public GameObject CharacterParts
    {
        get =>_characterParts;
        private set => _characterParts = value;
    }

    private void Awake()
    {
        _center = transform.Find(CENTER_NODE_NAME).gameObject;
        _characterParts = transform.Find(CHARACTER_PARTS_NAME).gameObject;
    }
}
