﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.TheInfernalChoir
{
    public class TheInfernalChoirCharacterCardController : VillainCharacterCardController
    {
        public TheInfernalChoirCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(() => !Card.IsFlipped ? FindCard("VagrantHeartPhase1") : FindCard("VagrantHeartPhase2"));
            SpecialStringMaker.ShowSpecialString(() => "This card is indestructible.").Condition = () => !Card.IsFlipped;
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();

            if (!Card.IsFlipped)
            {
                AddSideTrigger(AddEndOfTurnTrigger(tt => tt == TurnTaker, pca => PlayTheTopCardOfTheVillainDeckWithMessageResponse(pca), TriggerType.PlayCard));
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(TurnTaker, CharacterCard, c => !(IsGhost(c) || IsVillainTarget(c)), TargetType.All, 1, DamageType.Infernal));

                if (Game.IsAdvanced)
                {
                    AddSideTrigger(AddIncreaseDamageTrigger(dda => true, 1));
                }
            }
            else
            {
                AddSideTrigger(AddRedirectDamageTrigger(dda => dda.DamageSource.IsHero && IsVillainTarget(dda.Target) && Game.ActiveTurnTaker == TurnTaker, () => GameController.OrderTargetsByHighestHitPoints(c => c.IsHero, false, GetCardSource()).First()));
                AddSideTrigger(AddStartOfTurnTrigger(tt => tt == TurnTaker, pca => FlippedCardRemoval(pca), new[] { TriggerType.RemoveFromGame, TriggerType.PlayCard }));
                AddSideTrigger(AddEndOfTurnTrigger(tt => tt == TurnTaker, pca => FlippedRemovePlayedCards(pca), TriggerType.RemoveFromGame));

                if (Game.IsAdvanced)
                {
                    AddSideTrigger(AddStartOfTurnTrigger(tt => tt.IsHero, pca => AdvancedRemoveTopCardOfDeck(pca), TriggerType.RemoveFromGame));
                }

                AddDefeatedIfDestroyedTriggers();
            }
        }

        protected bool IsGhost(Card c, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        {
            return c != null && (c.DoKeywordsContain("ghost", evenIfUnderCard, evenIfFaceDown) || GameController.DoesCardContainKeyword(c, "ghost", evenIfUnderCard, evenIfFaceDown));
        }

        public override bool CanBeDestroyed => !Card.IsFlipped;

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator coroutine;
            var p1Heart = TurnTaker.FindCard("VagrantHeartPhase1", false);
            var p2Heart = TurnTaker.FindCard("VagrantHeartPhase2", false);
            var tt = p1Heart.Location.OwnerTurnTaker;
            var httc = FindHeroTurnTakerController(tt.ToHero());

            //We have to preremove the Triggers from the first heart or else it triggers incorrectly.
            var p1Cc = FindCardController(p1Heart);
            p1Cc.RemoveAllTriggers();

            coroutine = GameController.ShuffleCardsIntoLocation(HeroTurnTakerController, p1Heart.UnderLocation.Cards, tt.Deck, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = GameController.MoveCard(TurnTakerController, p1Heart, TurnTaker.OutOfGame, flipFaceDown: true, evenIfIndestructible: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            if (p2Heart.IsFlipped)
                p2Heart.SetFlipped(false);

            coroutine = GameController.MoveIntoPlay(TurnTakerController, p2Heart, tt, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.AfterFlipCardImmediateResponse();
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator FlippedCardRemoval(GameAction action)
        {
            IEnumerator coroutine;
            foreach (var httc in GameController.FindHeroTurnTakerControllers().Where(ht => !ht.IsIncapacitatedOrOutOfGame))
            {
                var htt = httc.HeroTurnTaker;
                if (htt.Deck.NumberOfCards > 5)
                {
                    var cards = htt.Deck.GetTopCards(htt.Deck.NumberOfCards - 5).ToList();
                    int count = cards.Count;
                    var msg = $"The terrible song of {Card.Title} removes {count} {count.ToString_SingularOrPlural("card", "cards")} from {htt.Deck.GetFriendlyName()}.";
                    coroutine = GameController.SendMessageAction(msg, Priority.Medium, GetCardSource(), showCardSource: true);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }

                    coroutine = GameController.BulkMoveCards(TurnTakerController, cards, htt.OutOfGame, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                }
                else if (!htt.Deck.HasCards)
                {
                    var msg = $"{htt.NameRespectingVariant} can no longer resist {Card.Title}'s spectral music....{{BR}}" +
                              $"[b]{htt.CharacterCards.Select(c => c.AlternateTitleOrTitle).ToCommaList(true)} will be destroyed.[/b]";
                    coroutine = GameController.SendMessageAction(msg, Priority.Critical, GetCardSource(), showCardSource: true);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }

                    coroutine = GameController.DestroyCards(httc, new LinqCardCriteria(c => c.IsHeroCharacterCard && !c.IsIncapacitatedOrOutOfGame && htt.CharacterCards.Contains(c)),
                                autoDecide: true,
                                cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }

            List<Card> playedCards = new List<Card>();
            coroutine = DoActionToEachTurnTakerInTurnOrder(ttc => !ttc.IsIncapacitatedOrOutOfGame && !ttc.TurnTaker.IsEnvironment, ttc => GameController.PlayTopCardOfLocation(ttc, ttc.TurnTaker.Deck, cardSource: GetCardSource(), playedCards: playedCards, showMessage: true));
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }

            foreach (var card in playedCards)
            {
                if (card.IsHero)
                    Journal.RecordCardProperties(card, "TheInfernalChoirRemoveFromGame", true);
            }
        }

        private IEnumerator FlippedRemovePlayedCards(GameAction action)
        {
            var cards = FindCardsWhere((Card c) => !c.IsOutOfGame && Journal.GetCardPropertiesBoolean(c, "TheInfernalChoirRemoveFromGame") == true, visibleToCard: GetCardSource());
            var coroutine = GameController.MoveCards(TurnTakerController, cards, c => new MoveCardDestination(c.Owner.OutOfGame, showMessage: true), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }

        private IEnumerator AdvancedRemoveTopCardOfDeck(PhaseChangeAction action)
        {
            var tt = action.ToPhase.TurnTaker;
            var coroutine = GameController.MoveCard(TurnTakerController, tt.Deck.TopCard, tt.OutOfGame, showMessage: true, actionSource: action, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}
