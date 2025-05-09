using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return true;
    }
}
