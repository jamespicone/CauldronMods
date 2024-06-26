﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using Cauldron.Malichae;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    public class MalichaeVariantTests : CauldronBaseTest
    {
        #region MalichaeTestsHelperFunctions
        private void SetupIncap(HeroTurnTakerController heroToIncap)
        {
            SetHitPoints(heroToIncap.CharacterCard, 1);
            DealDamage(GameController.FindVillainTurnTakerControllers(true).First(), heroToIncap, 2, DamageType.Melee);
            AssertIncapacitated(heroToIncap);
        }

        #endregion

        [Test()]
        public void TestShardmasterMalichaeLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/ShardmasterMalichaeCharacter", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(malichae);
            Assert.IsInstanceOf(typeof(ShardmasterMalichaeCharacterCardController), malichae.CharacterCardController);

            Assert.AreEqual(26, malichae.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestShardmasterMalichaePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/ShardmasterMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);

            QuickShuffleStorage(malichae);
            var card = malichae.TurnTaker.Deck.Cards.First(c => c.DoKeywordsContain("djinn"));
            DecisionSelectCard = card;

            UsePower(malichae.CharacterCard);

            AssertInHand(card);
            QuickShuffleCheck(1);
        }


        [Test()]
        public void TestShardmasterMalichaeIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/ShardmasterMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(malichae);

            GoToUseIncapacitatedAbilityPhase(malichae);

            var cs = StackDeck(ra, new[] { "ImbuedFire", "FireBlast" }).ToArray();
            AssertInDeck(cs[0]);
            AssertInDeck(cs[1]);

            DecisionSelectTurnTaker = ra.TurnTaker;
            DecisionSelectCards = new[] { cs[0], cs[1] };

            UseIncapacitatedAbility(malichae, 0);
            AssertInTrash(cs[0]);
            AssertInDeck(cs[1]);
            AssertOnTopOfDeck(cs[1]);

            AssertNumberOfCardsInRevealed(ra, 0);
        }

        [Test()]
        public void TestShardmasterMalichaeIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/ShardmasterMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(malichae);

            var c1 = PlayCard("LivingForceField");
            AssertIsInPlay(c1);

            GoToUseIncapacitatedAbilityPhase(malichae);

            DecisionSelectCard = c1;

            UseIncapacitatedAbility(malichae, 1);
            AssertInTrash(c1);
        }

        [Test()]
        public void TestShardmasterMalichaeIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/ShardmasterMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(malichae);

            GoToUseIncapacitatedAbilityPhase(malichae);

            DecisionSelectTurnTaker = ra.TurnTaker;

            QuickHandStorage(ra, haka);
            UseIncapacitatedAbility(malichae, 2);

            QuickHandCheck(1, 0);
        }



        [Test()]
        public void TestMinistryOfStrategicScienceMalichaeLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(malichae);
            Assert.IsInstanceOf(typeof(MinistryOfStrategicScienceMalichaeCharacterCardController), malichae.CharacterCardController);

            Assert.AreEqual(24, malichae.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestMinistryOfStrategicScienceMalichaePower()
        {
            SetupGameController(new string[] { "BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Haka", "Megalopolis" });
            StartGame();

            GoToUsePowerPhase(malichae);

            var card = PutInHand("Reshiel");
            var top = GetTopCardOfDeck(malichae);
            DecisionSelectCard = card;

            QuickHandStorage(malichae);
            UsePower(malichae.CharacterCard);
            QuickHandCheck(0);
            AssertInTrash(card);
            AssertInHand(top);
        }

        [Test()]
        public void TestMOSSMalichae_RepOfEarth_GreatestLegacy()
        {
            SetupGameController(new string[] { "BaronBlade", "Legacy/AmericasGreatestLegacyCharacter", "Ra", "Haka", "TheCelestialTribunal" });
            StartGame();
            DecisionSelectFromBoxIdentifiers = new string[] { "Cauldron.MinistryOfStrategicScienceMalichaeCharacter" };
            DecisionSelectFromBoxTurnTakerIdentifier = "Cauldron.Malichae";
            Card earth = PlayCard("RepresentativeOfEarth");

            Card rep = earth.NextToLocation.Cards.First();

            DecisionSelectCard = rep;
            UsePower(legacy.CharacterCard);

        }

        [Test()]
        public void TestMOSSMalichae_RepOfEarth_CallToJudgement()
        {
            SetupGameController(new string[] { "BaronBlade", "Legacy/AmericasGreatestLegacyCharacter", "Ra", "Haka", "TheCelestialTribunal" });
            StartGame();
            DestroyNonCharacterVillainCards();
            Card tornado = PutInHand("BlazingTornado");

            DecisionSelectFromBoxIdentifiers = new string[] { "Cauldron.MinistryOfStrategicScienceMalichaeCharacter" };
            DecisionSelectFromBoxTurnTakerIdentifier = "Cauldron.Malichae";

            DecisionSelectCards = new Card[] { ra.CharacterCard, tornado, baron.CharacterCard };
            QuickHPStorage(baron.CharacterCard);

            PlayCard("CalledToJudgement");
            QuickHPCheck(-3);


        }


        [Test]
        public void Djinn_HighReshiel_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);

            var card = PutInHand("HighReshiel");

            DecisionSelectCards = new Card[] { card, ra.CharacterCard, fanatic.CharacterCard, malichae.CharacterCard };
            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard);
            UsePower(malichae.CharacterCard);

            QuickHPCheck(0, -2, -2, -2); //2
            AssertInTrash(malichae, card);
        }

        public void Djinn_GrandBathiel_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);

            var card = PutInHand("GrandReshiel");

            DecisionSelectCards = new Card[] { card, ra.CharacterCard };
            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, card);
            UsePower(malichae.CharacterCard);

            QuickHPCheck(0, 0, -6, 0, 0); //6
            AssertInTrash(malichae, card);
        }


        [Test]
        public void Djinn_GrandEzael_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            SetHitPoints(ra.CharacterCard, 20);
            SetHitPoints(baron.CharacterCard, 20);
            SetHitPoints(fanatic.CharacterCard, 20);
            SetHitPoints(malichae.CharacterCard, 20);

            GoToUsePowerPhase(malichae);
            var card = PutInHand("GrandEzael");
            DecisionSelectCards = new Card[] { card };

            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard);
            UsePower(malichae.CharacterCard);

            QuickHPCheck(0, 3, 3, 3);
            AssertInTrash(malichae, card);
        }

        [Test]
        public void Djinn_GrandReshiel_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            var mdp = GetCardInPlay("MobileDefensePlatform");
            var blade = PlayCard("BladeBattalion");

            GoToUsePowerPhase(malichae);
            var card = PutInHand("GrandReshiel");

            DecisionSelectCards = new Card[] { card };
            DecisionAutoDecideIfAble = true;
            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, blade, mdp);
            UsePower(malichae.CharacterCard);

            QuickHPCheck(0, 0, 0, 0, -2, -2); //2 + 1
            AssertInTrash(malichae, card);
        }

        [Test]
        public void Djinn_GrandSomael_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            var mdp = GetCardInPlay("MobileDefensePlatform");
            var blade = PlayCard("BladeBattalion");

            GoToUsePowerPhase(malichae);
            var card = PutInHand("GrandSomael");

            DecisionSelectCards = new Card[] { card };

            DecisionAutoDecideIfAble = true;
            UsePower(malichae.CharacterCard);

            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, mdp, blade);
            DealDamage(baron.CharacterCard, malichae.CharacterCard, 2, DamageType.Cold);
            DealDamage(baron.CharacterCard, ra.CharacterCard, 2, DamageType.Cold);
            DealDamage(baron.CharacterCard, fanatic.CharacterCard, 2, DamageType.Cold);
            DealDamage(baron.CharacterCard, mdp, 2, DamageType.Cold);
            DealDamage(baron.CharacterCard, blade, 2, DamageType.Cold);
            QuickHPCheck(0, 0, 0, 0, -2, -2); //damage reduced to zero except for villain
            SetHitPoints(blade, 5);

            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, mdp, blade);
            DealDamage(baron.CharacterCard, malichae.CharacterCard, 2, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, ra.CharacterCard, 2, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, fanatic.CharacterCard, 2, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, mdp, 2, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, blade, 2, DamageType.Cold, true);
            QuickHPCheck(0, -2, -2, -2, -2, -2); //damage not reduced to zero
            SetHitPoints(blade, 5);

            DecisionSelectCard = ra.CharacterCard;

            AssertInTrash(malichae, card);
        }

        [Test]
        public void Djinn_HighBathiel_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);
            var card = PutInHand("HighBathiel");

            DecisionSelectCards = new Card[] { card, ra.CharacterCard };
            DecisionAutoDecideIfAble = true;
            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard);
            UsePower(malichae.CharacterCard);

            QuickHPCheck(0, 0, -4, 0); //4
            AssertInTrash(malichae, card);
        }


        [Test]
        public void Djinn_HighSomael_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            var blade = PlayCard("BladeBattalion");

            GoToUsePowerPhase(malichae);

            var card = PutInHand("HighSomael");

            DecisionSelectCards = new Card[] { card };
            DecisionAutoDecideIfAble = true;

            UsePower(malichae.CharacterCard);

            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, blade);
            DealDamage(baron.CharacterCard, malichae.CharacterCard, 1, DamageType.Cold);
            DealDamage(baron.CharacterCard, ra.CharacterCard, 1, DamageType.Cold);
            DealDamage(baron.CharacterCard, fanatic.CharacterCard, 1, DamageType.Cold);
            DealDamage(baron.CharacterCard, card, 1, DamageType.Cold);
            DealDamage(baron.CharacterCard, blade, 1, DamageType.Cold);
            QuickHPCheck(0, 0, 0, 0, -1); //damage reduced to zero except for villain

            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard, blade);
            DealDamage(baron.CharacterCard, malichae.CharacterCard, 1, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, ra.CharacterCard, 1, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, fanatic.CharacterCard, 1, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, card, 1, DamageType.Cold, true);
            DealDamage(baron.CharacterCard, blade, 1, DamageType.Cold, true);
            QuickHPCheck(0, -1, -1, -1, -1); //damage not reduced to zero

            AssertInTrash(malichae, card);
        }

        [Test]
        public void Djinn_HighEzael_UsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);

            SetHitPoints(ra.CharacterCard, 20);
            SetHitPoints(baron.CharacterCard, 20);
            SetHitPoints(fanatic.CharacterCard, 20);
            SetHitPoints(malichae.CharacterCard, 20);

            var card = PutInHand("HighEzael");

            DecisionSelectCards = new Card[] { card };
            DecisionAutoDecideIfAble = true;
            QuickHPStorage(baron.CharacterCard, malichae.CharacterCard, ra.CharacterCard, fanatic.CharacterCard);
            UsePower(malichae.CharacterCard);
            QuickHPCheck(0, 1, 1, 1);

            AssertInTrash(malichae, card);
        }


        [Test]
        public void SummoningCrystal()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            GoToUsePowerPhase(malichae);

            var card = PutInHand("SummoningCrystal");
            var target = PutInHand("Bathiel");

            DecisionSelectCards = new Card[] { card, target };
            UsePower(malichae.CharacterCard);

            AssertInPlayArea(malichae, target);
            AssertInTrash(malichae, card);
        }


        [Test]
        public void ZephaerensCompass()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Fanatic", "Megalopolis");
            StartGame();

            var ongoing = PlayCard("BacklashField");
            var envCard = PlayCard("PlummetingMonorail");

            GoToUsePowerPhase(malichae);

            var card = PutInHand("ZephaerensCompass");

            string djinn = "Reshiel";
            var target = PlayCard(djinn);
            DecisionMoveCard = target;
            var high = GetCard("High" + djinn);
            PlayCard(high);
            AssertNextToCard(high, target);

            DecisionSelectCards = new Card[] { card, ongoing, envCard };
            QuickHandStorage(malichae, ra, fanatic);

            UsePower(malichae.CharacterCard);
            
            QuickHandCheck(1, 0, 0);
            AssertInTrash(card);
            AssertInTrash(ongoing);
            AssertInTrash(envCard);
        }


        [Test()]
        public void TestMinistryOfStrategicScienceMalichaeIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();
            var mdp = GetMobileDefensePlatform().Card;

            SetupIncap(malichae);

            GoToUseIncapacitatedAbilityPhase(malichae);

            DecisionSelectTurnTaker = ra.TurnTaker;
            DecisionSelectCards = new Card[] { mdp };

            QuickHPStorage(mdp);
            UseIncapacitatedAbility(malichae, 0);

            QuickHPCheck(-2);
        }

        [Test()]
        public void TestMinistryOfStrategicScienceMalichaeIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(malichae);

            SetHitPoints(ra.CharacterCard, 20);
            SetHitPoints(haka.CharacterCard, 20);
            SetHitPoints(baron.CharacterCard, 20);

            GoToUseIncapacitatedAbilityPhase(malichae);

            QuickHPStorage(baron, ra, haka);
            DecisionSelectCards = new[] { ra.CharacterCard, haka.CharacterCard };
            UseIncapacitatedAbility(malichae, 1);

            QuickHPCheck(0, 1, 1);
        }

        [Test()]
        public void TestMinistryOfStrategicScienceMalichaeIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Malichae/MinistryOfStrategicScienceMalichaeCharacter", "Ra", "Haka", "Megalopolis");
            StartGame();

            SetupIncap(malichae);

            GoToUseIncapacitatedAbilityPhase(malichae);

            AssertNumberOfStatusEffectsInPlay(0);
            UseIncapacitatedAbility(malichae, 2);
            AssertNumberOfStatusEffectsInPlay(1);

            QuickHPStorage(ra.CharacterCard, haka.CharacterCard);
            DealDamage(baron, ra, 1, DamageType.Cold);
            QuickHPCheck(0, 0);

            QuickHPStorage(ra.CharacterCard, haka.CharacterCard);
            DealDamage(baron, haka, 2, DamageType.Cold);
            QuickHPCheck(0, -2);
        }

    }
}
