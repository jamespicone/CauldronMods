﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.ScreaMachine
{
    public class ValentineCharacterCardController : ScreaMachineBandCharacterCardController
    {
        public ValentineCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController, ScreaMachineBandmate.Value.Valentine)
        {
        }

        protected override string AbilityDescription => throw new NotImplementedException();

        protected override string UltimateFormMessage => "TODO";

        protected override IEnumerator ActivateBandAbility()
        {
            throw new NotImplementedException();
        }

        protected override void AddFlippedSideTriggers()
        {
            
        }
    }
}
