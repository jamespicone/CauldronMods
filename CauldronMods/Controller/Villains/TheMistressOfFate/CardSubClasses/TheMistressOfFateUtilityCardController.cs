﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheMistressOfFate
{
    public abstract class TheMistressOfFateUtilityCardController : CardController
    {
        private Location _dayDeck;
        private string StoredByDayKey = "MistressOfFateCardStoredByDayKey";

        private Card _storingDay;
        protected Card StoringDay
        {
            get
            {
                if(IsStoredCard)
                {
                    if(_storingDay == null)
                    {
                        _storingDay = GetCardPropertyJournalEntryCard(StoredByDayKey);
                    }
                    return _storingDay;
                }
                return null;
            }
        }

        protected bool? _isStoredCard;
        protected bool IsStoredCard
        {
            get
            {
                if(_isStoredCard == null)
                {
                    bool hasDayProperty = GetCardPropertyJournalEntryCard(StoredByDayKey) != null;
                    _isStoredCard = hasDayProperty;
                }
                return _isStoredCard ?? false;
            }
        }
        private Location dayDeck
        {
            get
            {
                if (_dayDeck == null)
                {
                    _dayDeck = TurnTaker.FindSubDeck("DayDeck");
                }
                return _dayDeck;
            }
        }
        protected TheMistressOfFateUtilityCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            var storingDayString = SpecialStringMaker.ShowSpecialString(() => $"This card recurs on the {StoringDay.Title}.");
            storingDayString.Condition = () => IsStoredCard;

            this.Card.UnderLocation.OverrideIsInPlay = false;
        }

        protected bool IsDay(Card c)
        {
            if(c != null && c.Definition.Keywords.Contains("day"))
            {
                return true;
            }
            return false;
        }

        public void SetStoringDay(Card day)
        {
            ClearStoringDay();
            _storingDay = day;
            _isStoredCard = true;
            AddCardPropertyJournalEntry(StoredByDayKey, day);
        }

        public void ClearStoringDay()
        {
            _storingDay = null;
            _isStoredCard = false;
            AddCardPropertyJournalEntry(StoredByDayKey, (Card)null);
        }

        protected bool IsDeckEmptier(GameAction ga)
        {
            if (ga is MoveCardAction mc)
            {
                return mc.CardToMove.IsVillain && mc.Destination != TurnTaker.Revealed && TurnTaker.Revealed.IsEmpty;
            }
            if (ga is PlayCardAction pc)
            {
                return pc.CardToPlay.IsVillain;
            }
            if (ga is DiscardCardAction dc)
            {
                return dc.Origin == TurnTaker.Deck;
            }
            return false;
        }

        protected List<Card> DayCardsInOrder()
        {
            return GameController.FindCardsWhere((Card c) => IsDay(c) && c.IsInPlay).OrderBy((Card c) => c.PlayIndex).ToList();
        }
    }
}
