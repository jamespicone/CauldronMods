﻿using NUnit.Framework;
using System;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Reflection;
using Cauldron.ScreaMachine;
using Handelabra;
using System.Collections.Generic;

namespace CauldronTests
{
    [TestFixture()]
    public class ScreaMachineTestsTests : BaseTest
    {
        #region ScreaMachineTestsHelperFunctions

        protected TurnTakerController scream { get { return FindVillain("ScreaMachine"); } }

        protected Card slice { get { return FindCardInPlay("SliceCharacter"); } }
        protected Card valentine { get { return FindCardInPlay("ValentineCharacter"); } }
        protected Card bloodlace { get { return FindCardInPlay("BloodlaceCharacter"); } }
        protected Card rickyg { get { return FindCardInPlay("RickyGCharacter"); } }
        protected Card setlist { get { return GetCard("TheSetList"); } }

        protected void AddImmuneToDamageTrigger(TurnTakerController ttc, bool heroesImmune, bool villainsImmune)
        {
            ImmuneToDamageStatusEffect immuneToDamageStatusEffect = new ImmuneToDamageStatusEffect();
            immuneToDamageStatusEffect.TargetCriteria.IsHero = new bool?(heroesImmune);
            immuneToDamageStatusEffect.TargetCriteria.IsVillain = new bool?(villainsImmune);
            immuneToDamageStatusEffect.UntilStartOfNextTurn(ttc.TurnTaker);
            this.RunCoroutine(this.GameController.AddStatusEffect(immuneToDamageStatusEffect, true, new CardSource(ttc.CharacterCardController)));
        }

        private void AssertHasKeyword(string keyword, IEnumerable<string> identifiers)
        {
            foreach (var id in identifiers)
            {
                var card = GetCard(id);
                AssertCardHasKeyword(card, keyword, false);
            }
        }

        private void AssertHasAbility(string abilityKey, IEnumerable<string> identifiers)
        {
            foreach (var id in identifiers)
            {
                var card = GetCard(id);
                int number = card.GetNumberOfActivatableAbilities(abilityKey);
                Assert.GreaterOrEqual(number, 1);
            }
        }

        private Card SetupBandCard(string identifier)
        {
            var card = FindCard(c => c.Identifier == identifier);
            if (card.Location == setlist.UnderLocation)
            {
                FlipCard(card);
                PlayCard(card);
            }
            return card;
        }

        private void AssertNumberOfActivatableAbility(Card card, string key, int number)
        {
            var cc = FindCardController(card);
            var abilities = cc.GetActivatableAbilities(key).Count();
            Assert.AreEqual(number, abilities, $"{card.Title} does not have the correct number of {(key is null ? "" : key)} abilities");
        }


        #endregion

        [Test()]
        public void TestScreaMachineLoadedProperly()
        {
            SetupGameController("Cauldron.ScreaMachine", "Legacy", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(scream);
            Assert.IsInstanceOf(typeof(ScreaMachineTurnTakerController), scream);

            Assert.IsNotNull(slice);
            Assert.IsInstanceOf(typeof(SliceCharacterCardController), FindCardController(slice));
            Assert.AreEqual(28, slice.HitPoints);

            Assert.IsNotNull(valentine);
            Assert.IsInstanceOf(typeof(ValentineCharacterCardController), FindCardController(valentine));
            Assert.AreEqual(31, valentine.HitPoints);

            Assert.IsNotNull(bloodlace);
            Assert.IsInstanceOf(typeof(BloodlaceCharacterCardController), FindCardController(bloodlace));
            Assert.AreEqual(26, bloodlace.HitPoints);

            Assert.IsNotNull(rickyg);
            Assert.IsInstanceOf(typeof(RickyGCharacterCardController), FindCardController(rickyg));
            Assert.AreEqual(35, rickyg.HitPoints);

            Assert.IsNotNull(setlist);
            Assert.IsInstanceOf(typeof(TheSetListCardController), FindCardController(setlist));
        }

        [Test()]
        public void TestScreaMachineDeckList()
        {
            SetupGameController("Cauldron.ScreaMachine", "Legacy", "Megalopolis");

            AssertHasKeyword("guitarist", new string[] {
                "ShredZone",
                "SlicesAxe"
            });
            AssertHasAbility("{Guitar}", new string[] {
                "ShredZone",
                "SlicesAxe"
            });
            AssertHasKeyword("bassist", new string[] {
                "Biosurge",
                "CantStopTheMusic"
            });
            AssertHasAbility("{Bass}", new string[] {
                "Biosurge",
                "CantStopTheMusic"
            });
            AssertHasKeyword("drummer", new string[] {
                "PoundingRhythm",
                "TectonicBeat"
            });
            AssertHasAbility("{Drum}", new string[] {
                "PoundingRhythm",
                "TectonicBeat"
            });
            AssertHasKeyword("vocalist", new string[] {
                "MentalLink",
                "IrresistibleVoice",
                "HypnotizeTheCrowd"
            });
            AssertHasAbility("{Vocal}", new string[] {
                "MentalLink",
                "IrresistibleVoice",
                "HypnotizeTheCrowd"
            });
            AssertHasKeyword("one-shot", new string[] {
                "HarshNote",
                "UpToEleven",
                "DeathMetal",
                "NothingButHits",
                "ScreamAndShout",
                "LiveInConcert"
            });
            AssertHasKeyword("ongoing", new string[] {
                "PercussiveWave"
            });

        }


        [Test()]
        public void TestScreaMachineGameStart()
        {

            SetupGameController("Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis");
            AssertInPlayArea(scream, setlist);
            AssertNotFlipped(setlist);
            QuickShuffleStorage(scream);
            StartGame();

            QuickShuffleCheck(1);
            AssertInPlayArea(scream, slice);
            AssertNotFlipped(slice);
            AssertInPlayArea(scream, bloodlace);
            AssertNotFlipped(bloodlace);
            AssertInPlayArea(scream, valentine);
            AssertNotFlipped(valentine);
            AssertInPlayArea(scream, rickyg);
            AssertNotFlipped(rickyg);
            AssertInPlayArea(scream, setlist);
            AssertFlipped(setlist);

            HashSet<string> _bandKeywords = new HashSet<string>(StringComparer.Ordinal)
            {
                "vocalist",
                "guitarist",
                "bassist",
                "drummer"
            };

            int inPlay = 0;
            foreach (var card in FindCardsWhere(c => GameController.GetAllKeywords(c, true, true).Any(str => _bandKeywords.Contains(str))))
            {
                if (card.Location != setlist.UnderLocation)
                {
                    AssertAtLocation(card, scream.TurnTaker.PlayArea);
                    inPlay++;
                }
            }

            Assert.AreEqual(1, inPlay, $"Should have 1 band cards in play");
        }

        [Test()]
        public void TestScreaMachineAdvancedGameStart()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis" }, advanced: true);
            AssertInPlayArea(scream, setlist);
            AssertNotFlipped(setlist);

            QuickShuffleStorage(scream.TurnTaker.Deck, setlist.UnderLocation);
            StartGame();
            QuickShuffleCheck(1, 1);

            AssertInPlayArea(scream, slice);
            AssertNotFlipped(slice);
            AssertInPlayArea(scream, bloodlace);
            AssertNotFlipped(bloodlace);
            AssertInPlayArea(scream, valentine);
            AssertNotFlipped(valentine);
            AssertInPlayArea(scream, rickyg);
            AssertNotFlipped(rickyg);
            AssertInPlayArea(scream, setlist);
            AssertFlipped(setlist);

            HashSet<string> _bandKeywords = new HashSet<string>(StringComparer.Ordinal)
            {
                "vocalist",
                "guitarist",
                "bassist",
                "drummer"
            };

            int inPlay = 0;
            foreach (var card in FindCardsWhere(c => GameController.GetAllKeywords(c, true, true).Any(str => _bandKeywords.Contains(str))))
            {
                if (card.Location != setlist.UnderLocation)
                {
                    AssertAtLocation(card, scream.TurnTaker.PlayArea);
                    inPlay++;
                }
            }

            Assert.AreEqual(this.GameController.Game.H - 2 + 1, inPlay, $"Should have {GameController.Game.H - 2 + 1} band cards in play");
        }

        [Test()]
        public void TestScreaMachineCharacterFlip([Values(ScreaMachineBandmate.Value.Slice, ScreaMachineBandmate.Value.Bloodlace, ScreaMachineBandmate.Value.Valentine, ScreaMachineBandmate.Value.RickyG)] ScreaMachineBandmate.Value member)
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis" }, advanced: false);
            StartGame();

            var memberCard = GetCard(member.GetIdentifier());
            AssertNotFlipped(memberCard);

            var cards = FindCardsWhere(c => c.DoKeywordsContain(member.GetKeyword(), true, true));
            foreach (var card in cards)
            {
                if (card.Location == setlist.UnderLocation)
                {
                    FlipCard(card);
                    PlayCard(card);
                }

                AssertNotFlipped(card);
                AssertInPlayArea(scream, card);
            }

            AssertFlipped(memberCard);
        }

        [Test()]
        public void TestSetListCharacterRemovedFromGameDestroy([Values(ScreaMachineBandmate.Value.Slice, ScreaMachineBandmate.Value.Bloodlace, ScreaMachineBandmate.Value.Valentine, ScreaMachineBandmate.Value.RickyG)] ScreaMachineBandmate.Value member)
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis" }, advanced: false);
            StartGame();

            var memberCard = GetCard(member.GetIdentifier());

            DestroyCard(memberCard);

            AssertAtLocation(memberCard, scream.TurnTaker.OutOfGame);
        }

        [Test()]
        public void TestSetListCharacterRemovedFromGameDamage([Values(ScreaMachineBandmate.Value.Slice, ScreaMachineBandmate.Value.Bloodlace, ScreaMachineBandmate.Value.Valentine, ScreaMachineBandmate.Value.RickyG)] ScreaMachineBandmate.Value member)
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis" }, advanced: false);
            StartGame();

            var memberCard = GetCard(member.GetIdentifier());

            DealDamage(legacy, memberCard, 99, DamageType.Cold);

            AssertAtLocation(memberCard, scream.TurnTaker.OutOfGame);
        }

        [Test()]
        public void TestSetListRevealCard()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Megalopolis" }, advanced: false);
            StartGame();
            GoToPlayCardPhase(scream);

            PrintSeparator("StartTest");

            List<Card> revealed = new List<Card>();
            //I use legacy as the revealer to confirm it's ANY reveal
            var cards = setlist.UnderLocation.GetTopCards(2).ToList();
            var c1 = cards[0];
            var c2 = cards[1];
            Console.WriteLine("Card to Reveal: " + c1.Title);

            RunCoroutine(GameController.RevealCards(legacy, setlist.UnderLocation, 1, revealed, cardSource: legacy.CharacterCardController.GetCardSource()));

            AssertNumberOfCardsInRevealed(scream, 0);

            if (c1.Location == setlist.UnderLocation)
            {
                Console.WriteLine($"{c1.Title} is under the setList");
                AssertOnBottomOfLocation(c1, setlist.UnderLocation);
                AssertInPlayArea(scream, c2);
            }
            else
            {
                Console.WriteLine($"{c1.Title} not under the setList");
                AssertInPlayArea(scream, c1);
                AssertOnTopOfLocation(c2, setlist.UnderLocation);
            }
        }

        [Test()]
        public void TestSetListEndOfTurn()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            //prevent card plays
            PlayCard("TakeDown");

            GoToPlayCardPhase(scream);
            DestroyCard(slice);
            DestroyCard(valentine);

            AssertInPlayArea(scream, rickyg);
            AssertInPlayArea(scream, bloodlace);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, rickyg, bloodlace);

            AssertNextMessage("Take Down prevented ScreaMachine from playing cards.");
            GoToEndOfTurn(scream);
            QuickHPCheck(-2, -2, -2, -2, 0, 0);

            //There's an end of turn card play here
        }

        [Test]
        [Ignore("Not Implemented")]
        public void TestSetListAdvancedDestroyCard([Values(ScreaMachineBandmate.Value.Slice, ScreaMachineBandmate.Value.Bloodlace, ScreaMachineBandmate.Value.Valentine, ScreaMachineBandmate.Value.RickyG)] ScreaMachineBandmate.Value member)
        {

        }


        [Test()]
        public void TestSliceAbility()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Slice);
            AssertNumberOfActivatableAbility(slice, key, 1);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, slice);
            QuickHPCheck(0, -3, 0, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestSliceUltimate()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Slice);

            FlipCard(slice);
            AssertNumberOfActivatableAbility(slice, key, 0);

            AssertNumberOfStatusEffectsInPlay(0);
            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            GoToEndOfTurn(scream);

            AssertNumberOfStatusEffectsInPlay(1);
            DealDamage(ra.CharacterCard, haka.CharacterCard, 2, DamageType.Cold); //cannot deal damage
            QuickHPCheck(0, -3, 0, 0, 0, 0, 0, 0);
        }


        [Test()]
        public void TestValentineAbility()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Valentine);
            AssertNumberOfActivatableAbility(valentine, key, 1);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, valentine);
            QuickHPCheck(-2, 0, -2, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestValentineUltimate()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Valentine);

            FlipCard(valentine);
            AssertNumberOfActivatableAbility(valentine, key, 0);
            //villain damage increased by 1

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            DealDamage(bloodlace, ra, 1, DamageType.Energy);
            QuickHPCheck(0, -2, 0, 0, 0, 0, 0, 0);

            QuickHPUpdate();
            GoToEndOfTurn(scream);
            //H -1 sonic + 1 & H - 2 + 1
            //3 + 1 & 2 + 1 = 7
            QuickHPCheck(0, 0, -7, 0, 0, 0, 0, 0);
        }


        [Test()]
        public void TestBloodlaceAbility()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Bloodlace);
            AssertNumberOfActivatableAbility(bloodlace, key, 1);

            SetHitPoints(slice, 10);
            SetHitPoints(bloodlace, 10);
            SetHitPoints(valentine, 10);
            SetHitPoints(rickyg, 10);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, bloodlace);
            QuickHPCheck(0, 0, 0, 0, 2, 0, 2, 2);
        }

        [Test()]
        public void TestBloodlaceUltimate()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Bloodlace);

            FlipCard(bloodlace);
            AssertNumberOfActivatableAbility(bloodlace, key, 0);

            //ensure ricky is the lowest
            SetHitPoints(rickyg, 10);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            GoToEndOfTurn(scream);

            //H damage, H healing
            QuickHPCheck(0, 0, -4, 0, 0, 0, 0, 4);
        }


        [Test()]
        public void TestRickyGAbility()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.RickyG);
            AssertNumberOfActivatableAbility(rickyg, key, 1);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            AssertNumberOfStatusEffectsInPlay(0);
            ActivateAbility(key, rickyg);
            AssertNumberOfStatusEffectsInPlay(1);

            DealDamage(ra, bloodlace, 2, DamageType.Radiant);
            QuickHPCheck(0, 0, 0, 0, 0, -1, 0, 0);
        }

        [Test()]
        public void TestRickyGUltimateImmunity()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.RickyG);

            FlipCard(rickyg);
            AssertNumberOfActivatableAbility(rickyg, key, 0);


            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            DealDamage(legacy, slice, 2, DamageType.Radiant, true);
            DealDamage(legacy, rickyg, 2, DamageType.Radiant);
            //slice immune to damage, rickyg is not
            QuickHPCheck(0, 0, 0, 0, 0, 0, 0, -2);

            //indestructible
            DestroyCard(slice);
            AssertInPlayArea(scream, slice);

            DestroyCard(rickyg);

            //immunity ends once ricky leaves play
            var s = slice; //FindCard fails once slice is destoryed, so buffer locally
            DestroyCard(s);
            AssertOutOfGame(s);

            QuickHPStorage(bloodlace);
            DealDamage(legacy, bloodlace, 5, DamageType.Cold);
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestRickyGUltimateDrumDiscard()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            FlipCard(rickyg);
            var card = PutOnDeck("UpToEleven");
            AssertInDeck(card);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            GoToEndOfTurn(scream);

            AssertInTrash(card);

            QuickHPCheck(-3, -3, -3, -3, 0, 0, 0, 0);
        }

        [Test()]
        public void TestRickyGUltimateNotDrumDiscard()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            PlayCard("TakeDown");

            FlipCard(rickyg);
            var card = PutOnDeck("DeathMetal");
            AssertInDeck(card);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            GoToEndOfTurn(scream);

            AssertInTrash(card);

            QuickHPCheck(0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        [Ignore("Not implemented")]
        public void TestBandCardRevealsIfBandMateMissing([Values(ScreaMachineBandmate.Value.Slice, ScreaMachineBandmate.Value.Bloodlace, ScreaMachineBandmate.Value.Valentine, ScreaMachineBandmate.Value.RickyG)] ScreaMachineBandmate.Value member)
        {

        }


        [Test()]
        public void TestShredZone()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            var card = SetupBandCard("ShredZone");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Slice);
            AssertNumberOfActivatableAbility(card, key, 1);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, card);
            QuickHPCheck(0, -1, 0, -1, 0, 0, 0, 0);
        }

        [Test()]
        public void TestSlicesAxe()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            var card = SetupBandCard("SlicesAxe");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Slice);
            AssertNumberOfActivatableAbility(card, key, 1);

            DrawCard(legacy); //make legacy the one with the most cards in hand

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, card);
            QuickHPCheck(-3, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test()]
        public void TestCantStopTheMusic()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            var card = SetupBandCard("CantStopTheMusic");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Bloodlace);
            AssertNumberOfActivatableAbility(card, key, 1);

            SetHitPoints(rickyg, 10);

            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, card);
            QuickHPCheck(0, 0, 0, 0, 0, 0, 0, 2);
        }


        [Test()]
        public void TestBiosurge()
        {
            SetupGameController(new[] { "Cauldron.ScreaMachine", "Legacy", "Ra", "Haka", "Bunker", "Megalopolis" }, advanced: false);
            StartGame();

            var card = SetupBandCard("Biosurge");

            string key = ScreaMachineBandmate.GetAbilityKey(ScreaMachineBandmate.Value.Bloodlace);
            AssertNumberOfActivatableAbility(card, key, 1);

            AssertNumberOfStatusEffectsInPlay(0);
            QuickHPStorage(legacy.CharacterCard, ra.CharacterCard, haka.CharacterCard, bunker.CharacterCard, slice, bloodlace, valentine, rickyg);
            ActivateAbility(key, card);
            AssertNumberOfStatusEffectsInPlay(1);

            //next bloodlace damage is increased, so not slice
            DealDamage(slice, ra, 1, DamageType.Lightning);
            DealDamage(bloodlace, legacy, 1, DamageType.Lightning);
            QuickHPCheck(-3, -1, 0, 0, 0, 0, 0, 0);

            //only next damage
            AssertNumberOfStatusEffectsInPlay(0);
        }

    }


}
