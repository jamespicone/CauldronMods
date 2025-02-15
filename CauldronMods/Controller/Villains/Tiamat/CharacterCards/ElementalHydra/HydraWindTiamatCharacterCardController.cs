﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Cauldron.Tiamat
{
    public class HydraWindTiamatCharacterCardController : HydraTiamatCharacterCardController
    {
        public HydraWindTiamatCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " is immune to Projectile damage.").Condition = () => !base.Card.IsFlipped;
        }

        protected override ITrigger[] AddFrontTriggers()
        {
            return new ITrigger[]
            { 
				//{Tiamat}, The Voice of the Wind is immune to Projectile damage.
				base.AddImmuneToDamageTrigger((DealDamageAction dealDamage) => dealDamage.Target == base.Card && dealDamage.DamageType == DamageType.Projectile),
                //At the end of the villain turn, each head regains 2 HP.
                base.AddEndOfTurnTrigger((TurnTaker turnTaker) => turnTaker == base.TurnTaker, GainHPResponse, TriggerType.GainHP)
            };
        }

        private IEnumerator GainHPResponse(PhaseChangeAction action)
        {
            IEnumerator coroutine = base.GameController.GainHP(this.DecisionMaker, (Card c) => IsHead(c), 2, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}
