﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Drift
{
    public class FutureFocusCardController : FocusUtilityCardController
    {
        public FutureFocusCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            //When {Drift} is dealt damage, if you have not shifted this turn, you may shift {DriftRRR}. If you shifted {DriftRRR} this way, {Drift} deals 1 target 3 radiant damage.
            base.AddTrigger<DealDamageAction>((DealDamageAction action) => action.Target == base.GetActiveCharacterCard() && action.Amount > 0 && !base.Journal.CardPropertiesEntriesThisRound((CardPropertiesJournalEntry entry) => entry.Key == HasShifted).Any(), this.ShiftResponse, TriggerType.ModifyTokens, TriggerTiming.After);
            ;
        }

        private IEnumerator ShiftResponse(DealDamageAction action)
        {
            bool canShift = base.CurrentShiftPosition() == 1;
            List<YesNoCardDecision> decision = new List<YesNoCardDecision>();
            //...you may shift {DriftRRR}. 
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.MakeDecision, this.Card, action, decision, new Card[] { base.GetShiftTrack() }, base.GetCardSource());
            if (decision.FirstOrDefault().Answer ?? false)
            {
                coroutine = base.ShiftRRR();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                //If you shifted {DriftRRR} this way, {Drift} deals 1 target 3 radiant damage.
                if (canShift)
                {
                    coroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.GetActiveCharacterCard()), 3, DamageType.Radiant, 1, false, 1, cardSource: base.GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
            yield break;
        }
    }
}
