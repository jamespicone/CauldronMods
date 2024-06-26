﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Linq;
using System;
using Handelabra;
using Cauldron.Cricket;

namespace Cauldron.Necro
{
    public abstract class NecroCardController : CardController
    {
        public static readonly string RitualKeyword = "ritual";
        public static readonly string UndeadKeyword = "undead";
        public static readonly string PastNecroPowerKey = "HeroVillainFlipped";

        protected NecroCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override void AddStartOfGameTriggers()
        {
            AddTrigger((GameAction ga) => TurnTakerController is NecroTurnTakerController ttc && !ttc.ArePromosSetup, SetupPromos, TriggerType.Hidden, TriggerTiming.Before, priority: TriggerPriority.High);
        }

        public IEnumerator SetupPromos(GameAction ga)
        {
            if (TurnTakerController is NecroTurnTakerController ttc && !ttc.ArePromosSetup)
            {
                ttc.SetupPromos(ttc.availablePromos);
                ttc.ArePromosSetup = true;
            }

            return DoNothing();
        }

        protected ITrigger AddUndeadDestroyedTrigger(Func<DestroyCardAction, IEnumerator> response, TriggerType triggerType)
        {
            return AddTrigger<DestroyCardAction>(d => this.IsUndead(d.CardToDestroy.Card) && d.WasCardDestroyed, response, triggerType, TriggerTiming.After);
        }

        protected bool IsHeroConsidering1929(Card card)
        {
            if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
            {
                return IsVillain(card);
            }
            return IsHero(card);
        }

        protected bool IsHeroTargetConsidering1929(Card card)
        {
            if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
            {
                return IsVillainTarget(card);
            }
            return IsHero(card) && card.IsTarget;
        }

        protected bool IsVillianConsidering1929(Card card)
        {
            if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
            {
                return IsHero(card);
            }
            return IsVillain(card);
        }

        protected bool IsVillianTargetConsidering1929(Card card)
        {
            if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
            {
                return IsHero(card) && card.IsTarget;
            }
            return IsVillainTarget(card);
        }

        protected string HeroStringConsidering1929
        {
            get
            {
                if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
                {
                    return "villain";
                }
                return "hero";
            }
        }

        protected string VillianStringConsidering1929
        {
            get
            {
                if (GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) != null && GameController.GetCardPropertyJournalEntryBoolean(CharacterCard, PastNecroPowerKey) == true)
                {
                    return "hero";
                }
                return "villian";
            }
        }

        protected bool IsRitual(Card card)
        {
            return card != null && card.DoKeywordsContain(RitualKeyword);
        }

        protected int GetNumberOfRitualsInPlay()
        {
            return FindCardsWhere(c => c.IsInPlayAndHasGameText && this.IsRitual(c)).Count();
        }

        protected bool IsUndead(Card card)
        {
            return card != null && card.DoKeywordsContain(UndeadKeyword);
        }
    }
}
