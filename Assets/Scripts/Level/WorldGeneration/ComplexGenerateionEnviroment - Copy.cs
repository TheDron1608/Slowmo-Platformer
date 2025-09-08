using UnityEngine;

public abstract class ComplexGenerateionEnviroment : MonoBehaviour
{
    public abstract void Generate();
    public abstract void PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk);
}