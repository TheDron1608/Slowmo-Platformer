using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class ComplexGenerateionEnviroment : MonoBehaviour
{
    public abstract List<GameObject> Generate();
    public abstract void PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk);
}