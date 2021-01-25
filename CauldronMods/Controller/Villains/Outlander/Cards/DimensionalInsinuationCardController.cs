﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Outlander
{
    public class DimensionalInsinuationCardController : OutlanderUtilityCardController
    {
        public DimensionalInsinuationCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            //When this card enters play, Search the villain deck for a copy of Anchored Fragment and put it into play.
            IEnumerable<Card> deckFragments = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == "AnchoredFragment" && c.Location.IsDeck && c.Location.IsVillain));
            if (deckFragments.Any())
            {
                coroutine = base.GameController.PlayCard(base.TurnTakerController, deckFragments.FirstOrDefault(), cardSource: base.GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
            }

            //Shuffle the villain deck...
            coroutine = base.ShuffleDeck(base.HeroTurnTakerController, base.TurnTaker.Deck);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            //...and play its top card.
            coroutine = base.GameController.PlayTopCard(base.DecisionMaker, base.TurnTakerController, cardSource: base.GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public override void AddTriggers()
        {
            //Damage dealt by {Outlander} is irreducible.
            base.AddMakeDamageIrreducibleTrigger((DealDamageAction action) => action.DamageSource.Card == base.CharacterCard);

            //At the start of the villain turn, destroy this card.
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, this.DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
