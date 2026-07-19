using System.Collections.Generic;
using UnityEngine;

public interface IStuckableObject
{
    public Collider2D StuckedToCollider { get; set; }
}
