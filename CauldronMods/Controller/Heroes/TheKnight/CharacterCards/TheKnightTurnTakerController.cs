﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheKnight
{ 
    public class TheKnightTurnTakerController : HeroTurnTakerController
    {
        private CharacterCardController _instructions = null;
        private bool? hasInstructionCard = null;

        public TheKnightTurnTakerController(TurnTaker tt, GameController gc) : base(tt, gc)
        {
        }
        private List<string> roninIdentifiers
        {
            get
            {
                return new List<string> { "TheYoungKnightCharacter", "TheOldKnightCharacter" };
            }
        }

        public List<Card> ManageCharactersOffToTheSide(bool banish = true)
        {
            List<Card> characterCards = new List<Card> { };
            foreach (string charID in roninIdentifiers)
            {
                Card target = TurnTaker.FindCard(charID);
                characterCards.Add(target);

                Location currentLoc = target.Location;

                if (currentLoc.IsOffToTheSide)
                {
                    Location destination;
                    //Log.Debug("Looking for destination...");
                    if (banish)
                    {
                        //Log.Debug("Attempting to banish...");
                        destination = TurnTaker.InTheBox;
                    }
                    else
                    {
                        //Log.Debug("Putting in play...");
                        destination = TurnTaker.PlayArea;
                    }
                    TurnTaker.MoveCard(target, destination);
                }

            }
            return characterCards;
        }
        public CharacterCardController InstructionCardController
        {
            get
            {
                if (hasInstructionCard == null)
                {
                    Card card = base.TurnTaker.FindCard("TheKnightCharacter", realCardsOnly: false);
                    //Log.Debug("Card found, promo-ID is " + card.PromoIdentifierOrIdentifier);
                    if (card != null && card.PromoIdentifierOrIdentifier == "WastelandRoninTheKnightCharacter")
                    {
                        //Log.Debug("Have found instruction card");
                        _instructions = (CharacterCardController)FindCardController(card);
                        hasInstructionCard = true;

                    }
                    else
                    {
                        //Log.Debug("No instruction card found");
                        hasInstructionCard = false;
                    }
                }
                return (bool)hasInstructionCard ? _instructions : null;
            }
        }
        public override Card CharacterCard
        {
            get
            {
                if (InstructionCardController != null)
                {
                    if (this.HasMultipleCharacterCards)
                    {
                        Log.Warning("There was a request for the Wasteland Ronin Knights' character card, which should be null.");
                        return null;
                    }
                    else
                    {
                        Log.Debug("There was a request for the Wasteland Ronin Knights' character card before setup was complete. Returning instruction card to avoid potential null reference errors.");
                    }
                }
                return base.CharacterCard;
            }
        }

        public override CharacterCardController IncapacitationCardController
        {
            get
            {
                if (InstructionCardController == null)
                {
                    return base.IncapacitationCardController;
                }
                else
                {
                    return InstructionCardController;
                }
            }
        }

        public override bool IsIncapacitated
        {
            get
            {
                if (InstructionCardController == null)
                {
                    return base.IsIncapacitated;
                }
                else 
                {
                    if (FindCardsWhere((Card c) => c.Owner == base.TurnTaker && c.IsActive && c.IsInPlayAndHasGameText).Count() != 0)
                    {
                        return IncapacitationCardController.Card.IsFlipped;
                    }
                    return true;
                }
            }
        }
        public override bool IsIncapacitatedOrOutOfGame
        {
            get
            {
                if (!IsIncapacitated)
                {
                    return IncapacitationCardController.Card.IsOutOfGame;
                }
                return true;
            }
        }

    }
}