﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.DungeonsOfTerror
{
    public class HighGroundCardController : DungeonsOfTerrorUtilityCardController
    {
        public HighGroundCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => BuildTopCardOfLocationSpecialString(TurnTaker.Trash));

        }

        public override void AddTriggers()
        {
            //While the top card of the environment trash is a fate card, reduce damage dealt by hero targets by 1. 
            AddReduceDamageTrigger((DealDamageAction dd) => IsTopCardOfLocationFate(TurnTaker.Trash) != null && IsTopCardOfLocationFate(TurnTaker.Trash).Value == true && dd.DamageSource.IsHero && dd.DamageSource.IsTarget,(DealDamageAction dd) => 1);

            //While it is not a fate card, reduce damage dealt by villain targets by 1.
            AddReduceDamageTrigger((DealDamageAction dd) => IsTopCardOfLocationFate(TurnTaker.Trash) != null && IsTopCardOfLocationFate(TurnTaker.Trash).Value == false && dd.DamageSource.IsVillainTarget, (DealDamageAction dd) => 1);

            //At the start of the environment turn, destroy this card.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
