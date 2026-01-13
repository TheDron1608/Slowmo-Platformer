using System.Collections.Generic;
using UnityEngine;

public abstract class ComplexGenerateionEnviroment : MonoBehaviour
{
    public class PreGeneratedEnviromentTempInfo
    {
        public ZIndexLayer GenerateWhere;
        public Vector3 Offset;
        public ComplexGenerateionEnviroment TargetGeneration;
        public BuildingInfo Building;
        public ChunkInfo Chunk;

        public bool Generated = false;

        public PreGeneratedEnviromentTempInfo(ZIndexLayer generateWhere, Vector3 offset, ComplexGenerateionEnviroment targetGeneration, BuildingInfo building, ChunkInfo chunk)
        {
            GenerateWhere = generateWhere;
            Offset = offset;
            TargetGeneration = targetGeneration;
            Building = building;
            Chunk = chunk;

            GenerateWhere.GenerationTempInfo.Add(this);
        }

        public List<GameObject> Generate()
        {
            return TargetGeneration.Generate(this);
        }

        public virtual void Remove()
        {
            GenerateWhere.GenerationTempInfo.Remove(this);
        }

        public Vector3 GetSpawnPosition()
        {
            return Offset + TargetGeneration.transform.position;
        }

        public Vector3Int GetTileSpawnPosition()
        {
            return VectorMath.Vec3ToVec3Int(Offset + TargetGeneration.transform.position);
        }
    }

    private ChunkInfo _chunk;
    private BuildingInfo _building;

    public ChunkInfo Chunk
    {
        get => _chunk;
        private set => _chunk = value;
    }
    public BuildingInfo Building
    {
        get => _building;
        private set => _building = value;
    }


    public virtual List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        generationInfo.Generated = true;
        return null;
    }
    public virtual PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        PreGeneratedEnviromentTempInfo newTempInfo = new PreGeneratedEnviromentTempInfo(preGenerateWhere, position, this, building, chunk);
        return newTempInfo;
    }
}