﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.CatchwaterHarbor
{
    public class RadioPlazaCardController : CatchwaterHarborUtilityCardController
    {
        public RadioPlazaCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => $"The top card of {TurnTaker.Deck.GetFriendlyName()} is {TurnTaker.Deck.TopCard.Title}.").Condition = () => Card.IsInPlayAndHasGameText && TurnTaker.Deck.NumberOfCards > 0;
        }

        public override void AddStartOfGameTriggers()
        {
            BuildTopDeckSpecialStrings();
        }

        private void BuildTopDeckSpecialStrings()
        {
            IEnumerable<TurnTaker> activeTurnTakers =  FindTurnTakersWhere((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && !tt.IsEnvironment);
            foreach(TurnTaker tt in activeTurnTakers)
            {
                foreach(Location deck in tt.Decks)
                {
                    SpecialStringMaker.ShowSpecialString(() => $"The top card of {deck.GetFriendlyName()} is {deck.TopCard.Title}.", relatedCards: () => tt.CharacterCards.Where(c => c.IsInPlayAndHasGameText)).Condition = () => Card.IsInPlayAndHasGameText && deck.NumberOfCards > 0;
                }
            }
        }

        public override void AddTriggers()
        {
            //Damage dealt to hero targets is irreducible.
            AddMakeDamageIrreducibleTrigger((DealDamageAction dd) => dd.Target.IsHero && GameController.IsCardVisibleToCardSource(dd.Target, GetCardSource()));
            //At the start of the environment turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);

        }
    }
}
