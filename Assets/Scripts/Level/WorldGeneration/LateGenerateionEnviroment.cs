using UnityEngine;
using UnityEngine.UIElements;

public abstract class LateGenerateionEnviroment : ComplexGenerateionEnviroment
{
    public override void PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        GameObject newObject = Instantiate(
            gameObject,
            gameObject.transform.position + position,
            gameObject.transform.rotation,
            preGenerateWhere.WorldGenerationDataObjectsContainer
            );
        LayerManager.Instance.ChangeZIndexForGameObject(LayerManager.Instance.GetZLayerOfGameObject(newObject), newObject);
        LayerManager.Instance.GetZLayerOfGameObject(newObject).UpdateLayerForGameObject(newObject);
    }
}