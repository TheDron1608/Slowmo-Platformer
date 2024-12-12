using System.Collections.Generic;
using UnityEngine;

public class CollidersInsideTracker : MonoBehaviour
{
    const string IGNORE_TRACKER_TAG_NAME = "IgnoreCollidersInsideTracker";

    private List<Collision2D> _collisionsInside = new();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag != IGNORE_TRACKER_TAG_NAME)
        {
            _collisionsInside.Add(collision);
            Debug.Log(collision.gameObject.name);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag != IGNORE_TRACKER_TAG_NAME)
        {
            _collisionsInside.Remove(collision);
            Debug.Log(collision.gameObject.name);
        }
    }

    public List<Collision2D> GetCollisionsInside()
    {
        return _collisionsInside;
    }

    public bool HasAnyCollisionsInside()
    {
        return _collisionsInside.Count > 0;
    }
}
