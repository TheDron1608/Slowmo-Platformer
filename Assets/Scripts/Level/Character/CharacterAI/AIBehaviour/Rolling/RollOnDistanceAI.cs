public class RollOnDistanceAI : AbstractAIRolling
{
    public float DistanceToRoll = 3f;
    public float DistanceToPrepareRollAgain = 10f;
    public int MaxRollsCombo = 1;
    public bool InvertRollDirection = false;

    private int _rollsComboLeft;

    protected override void OnAwake()
    {
        base.OnAwake();
        _rollsComboLeft = MaxRollsCombo;
    }

    private void FixedUpdate()
    {
        if (CharComponents.CharacterRolling.IsRolling) return;

        if (_rollsComboLeft > 0)
        {
            if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance <= DistanceToRoll)
            {
                CharComponents.CharacterRolling.TryRoll(
                    transform.position.x > _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.transform.position.x ^ InvertRollDirection ? 1f : -1f
                    );

                _rollsComboLeft--;
            }
        }
        else
        {
            if (
                _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy == null ||
                _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance >= DistanceToPrepareRollAgain
                )
            {
                _rollsComboLeft = MaxRollsCombo;
            }
        }
    }
}
