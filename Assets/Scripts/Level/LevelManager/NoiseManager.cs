using System;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public class OnNoiseCommitedEventArgs
    {
        public Vector2 Position;
        public ZIndexLayer Layer;
        public float Distance;
        public GameObject Source;
        public CharacterTeam SourceTeam;

        public OnNoiseCommitedEventArgs(Vector2 position, ZIndexLayer layer, float distance, GameObject source, CharacterTeam sourceTeam)
        {
            Position = position;
            Layer = layer; 
            Distance = distance; 
            Source = source;
            SourceTeam = sourceTeam;
        }
    }

    public static NoiseManager Instance;

    public event EventHandler<OnNoiseCommitedEventArgs> OnNoiseCommited;

    public float GlobalNoiseMult = 1f;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 NoiseManager per scene");
        Instance = this;
    }

    public void CommitNoise(Vector2 position, ZIndexLayer layer, float distance, GameObject source, CharacterTeam sourceTeam)
    {
        OnNoiseCommited?.Invoke(this, new(position, layer, distance * GlobalNoiseMult, source, sourceTeam));
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}