﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Menagerie
{
    public class MenagerieCharacterCardController : VillainCharacterCardController
    {
        public MenagerieCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }
        public override bool AllowFastCoroutinesDuringPretend
        {
            get
            {
                return !base.Card.IsFlipped;
            }
        }

        private bool IsEnclosure(Card c)
        {
            return c.DoKeywordsContain("enclosure");
        }

        public bool IsCaptured(TurnTaker tt)
        {
            Card prize = FindCard("PrizedCatch");
            return prize.Location.IsNextToCard && tt.GetAllCards().Contains(base.GetCardThisCardIsNextTo());
        }

        public bool HasEnclosure(TurnTaker tt)
        {
            return base.FindCardsWhere(new LinqCardCriteria((Card c) => this.IsEnclosure(c) && tt == c.Location.OwnerTurnTaker)).Any();
        }

        public override void AddSideTriggers()
        {
            if (!base.Card.IsFlipped)
            { //Front
                //Cards beneath villain cards are not considered in play. When an enclosure leaves play, put it under this card... 
                base.AddSideTrigger(base.AddTrigger<DestroyCardAction>((DestroyCardAction action) => this.IsEnclosure(action.CardToDestroy.Card) && action.PostDestroyDestinationCanBeChanged && action.CardToDestroy.CanBeDestroyed && action.WasCardDestroyed, this.PutUnderThisCardResponse, TriggerType.MoveCard, TriggerTiming.Before));
                base.AddSideTrigger(base.AddTrigger<MoveCardAction>((MoveCardAction action) => this.IsEnclosure(action.CardToMove) && action.CanChangeDestination && action.Origin.IsInPlayAndNotUnderCard, this.PutUnderThisCardResponse, TriggerType.MoveCard, TriggerTiming.Before));
                //...discarding all cards beneath it. Put any discarded targets into play.
                /**logic added to enclosures**/

                //At the end of the villain turn, reveal cards from the top of the villain deck until an enclosure is revealed, play it, and shuffle the other revealed cards back into the deck. Then if {H} enclosures are beneath this card, flip {Menagerie}'s character cards.
                base.AddSideTrigger(base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, this.PlayEnclosureMaybeFlipResponse, new TriggerType[] { TriggerType.PlayCard }));

                //Prized Catch is Indestructible. The heroes lose if the captured hero is incapacitated.
                /**This is on Prized Catch**/
                if (base.Game.IsAdvanced)
                { //Front - Advanced
                    //When an enclosure enters play, put the top card of the villain deck beneath it.
                    base.AddSideTrigger(base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction action) => action.IsSuccessful && this.IsEnclosure(action.CardEnteringPlay), this.EncloseExtraCard, TriggerType.MoveCard, TriggerTiming.After));
                }
            }
            else
            { //Back
                //When an enclosure enters play, move it next to the active hero with the fewest enclosures in their play area. Heroes with enclosures in their play area may not damage cards in other play areas.
                /**logic added to enclosures**/

                //Cards beneath enclosures are not considered in play. When an enclosure leaves play, discard all cards beneath it.
                /**logic added to enclosures**/

                //At the end of the villain turn, play the top card of the villain deck. Then, for each enclosure in play, Menagerie deals the hero next to it X projectile damage, where X is the number of cards beneath that enclosure.
                base.AddSideTrigger(base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, this.PlayCardDealDamageResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DealDamage }));
                if (base.Game.IsAdvanced)
                { //Back - Advanced
                    //At the start of the villain turn, if each active hero has an enclosure in their play area, the heroes lose the game.
                }
            }
            base.AddDefeatedIfDestroyedTriggers();
        }

        private IEnumerator PutUnderThisCardResponse(DestroyCardAction action)
        {
            action.SetPostDestroyDestination(base.Card.UnderLocation, cardSource: base.GetCardSource());
            yield break;
        }

        private IEnumerator PutUnderThisCardResponse(MoveCardAction action)
        {
            action.SetDestination(base.CharacterCard.UnderLocation);
            yield break;
        }

        private IEnumerator PlayEnclosureMaybeFlipResponse(PhaseChangeAction action)
        {
            //...reveal cards from the top of the villain deck until an enclosure is revealed, play it, and shuffle the other revealed cards back into the deck. 
            IEnumerator coroutine = base.RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, true, false, false, new LinqCardCriteria((Card c) => this.IsEnclosure(c)), 1, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards, shuffleReturnedCards: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then if {H} enclosures are beneath this card, flip {Menagerie}'s character cards.
            if (base.FindCardsWhere((new LinqCardCriteria((Card c) => this.IsEnclosure(c) && c.Location == base.Card.UnderLocation))).Count() >= Game.H)
            {
                coroutine = base.FlipThisCharacterCardResponse(action);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }

        private IEnumerator EncloseExtraCard(CardEntersPlayAction action)
        {
            //...enclosure enters play, put the top card of the villain deck beneath it.
            IEnumerator coroutine = base.GameController.MoveCard(base.TurnTakerController, base.TurnTaker.Deck.TopCard, action.CardEnteringPlay.UnderLocation, flipFaceDown: true, cardSource: base.GetCardSource());
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

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            //When Menagerie flips to this side, shuffle the villain trash and all enclosurese beneath this card into the villain deck. 
            IEnumerator coroutine = base.GameController.MoveCards(base.TurnTakerController, base.FindCardsWhere(new LinqCardCriteria((Card c) => (this.IsEnclosure(c) && c.Location == base.Card.UnderLocation) || c.Location == base.TurnTaker.Trash)), base.TurnTaker.Deck, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Remove Prized Catch from the game.
            coroutine = base.GameController.MoveCard(base.TurnTakerController, base.FindCard("PrizedCatch"), base.TurnTaker.OutOfGame, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            base.RemoveAllTriggers();
            this.AddSideTriggers();
            yield break;
        }

        private IEnumerator PlayCardDealDamageResponse(PhaseChangeAction action)
        {
            //...play the top card of the villain deck. 
            IEnumerator coroutine = base.PlayTheTopCardOfTheVillainDeckResponse(action);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            //Then, for each enclosure in play, Menagerie deals the hero next to it X projectile damage, where X is the number of cards beneath that enclosure.
            IEnumerable<Card> enclosures = base.FindCardsWhere(new LinqCardCriteria((Card c) => this.IsEnclosure(c) && c.IsInPlayAndHasGameText && c.Location.IsHeroPlayAreaRecursive));
            foreach (Card enclosure in enclosures)
            {
                coroutine = base.DealDamage(base.Card, (Card c) => c == enclosure.Location.OwnerCard, (Card c) => enclosure.UnderLocation.NumberOfCards, DamageType.Projectile);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }
    }
}