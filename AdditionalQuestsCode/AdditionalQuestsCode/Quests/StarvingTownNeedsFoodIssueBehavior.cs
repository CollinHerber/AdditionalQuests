﻿using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace AdditionalQuestsCode.Quests
{
    public class StarvingTownNeedsFoodIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be starving town, also merchant notable
        private bool ConditionsHold(Hero issueGiver)
        {
            return issueGiver.IsMerchant && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsTown && issueGiver.CurrentSettlement.IsStarving;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.VeryCommon));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodIssue(issueOwner);
        }

        // Now the Issue
        internal class StarvingTownNeedsFoodIssue : IssueBase
        {
            public StarvingTownNeedsFoodIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT} needs food", null);
                    textObject.SetTextVariable("SETTLEMENT", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("The Town of {SETTLEMENT} is starving, {MERCHANT_NAME} needs help to restock food and help the people.", null);
                    textObject.SetTextVariable("SETTLEMENT", base.IssueSettlement.Name);
                    textObject.SetTextVariable("MERCHANT_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("The situation is bad, the town is starving, we've had to ration out all the food coming into the city but its getting harder and harder each day for the people. If this keeps up for much longer, we will either all starve to death or the people will riot.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("Can I help? I have some food to spare.", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("I thank you for your kindness, but we require a much larger amount of food to restock the city, enough to keep us going while we sort out the source of our food shortage. If you can supply Grain, Meat and Fish, I will pay you double for each you can bring us. Don't worry, the nobles are paying for this, so it won't be out of the peoples pocket.", null);
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will head out to find food for the town.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("I'm so hungry... oh, sorry, Did I walk into you? Its been so hard lately... I haven't eaten in so long... If only someone could help {QUEST_GIVER.NAME} bring more food into town...", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject);
                    return textObject;
                }
            }

            public override bool IsThereAlternativeSolution
            {
                get
                {
                    return false;
                }
            }

            public override bool IsThereLordSolution
            {
                get
                {
                    return false;
                }
            }

            public override IssueFrequency GetFrequency()
            {
                return IssueBase.IssueFrequency.Common;
            }

            public override bool IssueStayAliveConditions()
            {
                return IssueSettlement.IsStarving;
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                relationHero = null;
                flag = IssueBase.PreconditionFlags.None;
                if (issueGiver.GetRelationWithPlayer() < -10f)
                {
                    flag |= IssueBase.PreconditionFlags.Relation;
                    relationHero = issueGiver;
                }
                if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
                {
                    flag |= IssueBase.PreconditionFlags.AtWar;
                }
                skill = null;
                return flag == IssueBase.PreconditionFlags.None;
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                throw new NotImplementedException();
            }

            protected override void OnGameLoad()
            {
                throw new NotImplementedException();
            }
        }

        internal class StarvingTownNeedsFoodQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public StarvingTownNeedsFoodQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
            {
                this.SetDialogs();
                base.InitializeQuestOnCreation();
            }


            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{ISSUE_SETTLEMENT} needs spears for militia", null);
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK}, the headman of the {QUEST_SETTLEMENT} asked you to deliver {SPEARS_AMOUNT} spears to {?QUEST_GIVER.GENDER}her{?}him{\\?} for the village militia. This will help boost the number of able militia in the village. \n \n You have agreed to bring them {SPEARS_AMOUNT} spears as soon as possible.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerHasSpearsLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You now have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject StageTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to deliver {SPEARS_AMOUNT} spears to the villagers. They wont be able to properly train their militia. The Headman is disappointed.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered {SPEARS_AMOUNT} spears to the villagers. Their militia is ready to train and defend the village. The Headman and the villagers are grateful.", null);
                    textObject.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with the {ISSUE_GIVER.LINK}'s lord. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToRaidLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{SETTLEMENT_NAME} was raided by someone else. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
                    textObject.SetTextVariable("SETTLEMENT_NAME", base.QuestGiver.CurrentSettlement.Name);
                    StringHelpers.SetCharacterProperties("ISSUE_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }


            // Register Events
            protected override void RegisterEvents()
            {
                CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, new Action<List<ValueTuple<ItemRosterElement, int>>, List<ValueTuple<ItemRosterElement, int>>, bool>(this.OnPlayerInventoryExchange));
                CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction>(this.OnWarDeclared));
                CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
                CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, new Action<BattleSideEnum, MapEvent>(this.OnRaidCompleted));
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                bool flag = false;
                foreach (ValueTuple<ItemRosterElement, int> valueTuple in purchasedItems)
                {
                    ItemRosterElement item = valueTuple.Item1;
                    if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedPolearm)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (ValueTuple<ItemRosterElement, int> valueTuple2 in soldItems)
                    {
                        ItemRosterElement item = valueTuple2.Item1;
                        if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.OneHandedPolearm)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this.PlayerAcceptedQuestLog.UpdateCurrentProgress(this.GetRequiredSpearsCountOnPlayer());
                    this.CheckIfPlayerReadyToReturnSpears();
                }
            }

            private void OnMercenaryClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
            {
                this.CheckWarDeclaration();
            }

            private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
            {
                this.CheckWarDeclaration();
            }

            private void OnWarDeclared(IFaction faction1, IFaction faction2)
            {
                this.CheckWarDeclaration();
            }

            private void CheckWarDeclaration()
            {
                if (base.QuestGiver.CurrentSettlement.OwnerClan.IsAtWarWith(Clan.PlayerClan))
                {
                    base.CompleteQuestWithCancel(this.StageCancelDueToWarLogText);
                }
            }

            private void OnRaidCompleted(BattleSideEnum battleSide, MapEvent mapEvent)
            {
                if (mapEvent.MapEventSettlement == base.QuestGiver.CurrentSettlement)
                {
                    base.CompleteQuestWithCancel(this.StageCancelDueToRaidLogText);
                }
            }

            private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
            {
                QuestHelper.CheckMinorMajorCoercionAndFailQuest(this, mapEvent, attackerParty);
            }


            // Quest logic, the dialogs and conditions for it be to success or failure
            public override bool IsRemainingTimeHidden
            {
                get
                {
                    return false;
                }
            }

            protected override void InitializeQuestOnGameLoad()
            {
                this.SetDialogs();
            }

            protected override void SetDialogs()
            {
                TextObject thankYouText = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! We will make good use of these weapons.", null);
                thankYouText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                TextObject waitingText = new TextObject("We await those weapons, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                waitingText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);


                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(thankYouText, null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("Have you brought {SPEARS_AMOUNT} spears?", null), null, null).Condition(delegate
                {
                    MBTextManager.SetTextVariable("SPEARS_AMOUNT", this.NeededSpears);
                    return CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("Yes. Here they are.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.ReturnSpearsClickableConditions)).NpcLine(thankYouText, null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("I'm working on it.", null), null).NpcLine(waitingText, null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private bool ReturnSpearsClickableConditions(out TextObject explanation)
            {
                if (this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededSpears)
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("You don't have enough spears.", null);
                return false;
            }

            protected override void OnTimedOut()
            {
                base.AddLog(this.StageTimeoutLogText, false);
                this.Fail();
            }

            private void QuestAcceptedConsequences()
            {
                base.StartQuest();
                int requiredSpearsCountOnPlayer = this.GetRequiredSpearsCountOnPlayer();
                this.PlayerAcceptedQuestLog = base.AddDiscreteLog(this.StageOnePlayerAcceptsQuestLogText, new TextObject("Collect one handed polearms", null), requiredSpearsCountOnPlayer, this.NeededSpears, null, false);
            }

            private int GetRequiredSpearsCountOnPlayer()
            {
                int itemNumber = AdditionalQuestsHelperMethods.GetRequiredWeaponWithTypeCountOnPlayer(WeaponClass.OneHandedPolearm);
                if (itemNumber > this.NeededSpears)
                {
                    TextObject textObject = new TextObject("You have enough spears to complete the quest. Return to {QUEST_SETTLEMENT} to hand them over.", null);
                    textObject.SetTextVariable("QUEST_SETTLEMENT", base.QuestGiver.CurrentSettlement.Name);
                    InformationManager.AddQuickInformation(textObject, 0, null, "");
                }
                return itemNumber;
            }

            private void CheckIfPlayerReadyToReturnSpears()
            {
                if (this.PlayerHasNeededSpearsLog == null && this.PlayerAcceptedQuestLog.CurrentProgress >= this.NeededSpears)
                {
                    this.PlayerHasNeededSpearsLog = base.AddLog(this.StageTwoPlayerHasSpearsLogText, false);
                    return;
                }
                if (this.PlayerHasNeededSpearsLog != null && this.PlayerAcceptedQuestLog.CurrentProgress < this.NeededSpears)
                {
                    base.RemoveLog(this.PlayerHasNeededSpearsLog);
                    this.PlayerHasNeededSpearsLog = null;
                }
            }

            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this.StageSuccessLogText, false);
                TraitLevelingHelper.OnIssueSolvedThroughQuest(base.QuestGiver, new Tuple<TraitObject, int>[]
                {
                    new Tuple<TraitObject, int>(DefaultTraits.Mercy, 50),
                    new Tuple<TraitObject, int>(DefaultTraits.Generosity, 30)
                });
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, this.RewardGold, false);
                // Remove spears
                AdditionalQuestsHelperMethods.RemoveWeaponsWithTypeFromPlayer(WeaponClass.OneHandedPolearm, NeededSpears);
                base.QuestGiver.AddPower(25f);
                Settlement.CurrentSettlement.Prosperity += 50f;
                Settlement.CurrentSettlement.Militia += 20f;
                this.RelationshipChangeWithQuestGiver = 10;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            private void Fail()
            {
                base.QuestGiver.AddPower(-5f);
                base.QuestGiver.CurrentSettlement.Prosperity += -10f;
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }


            // Saved vars/logs
            [SaveableField(10)]
            private readonly int NeededSpears;

            [SaveableField(20)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(30)]
            private JournalLog PlayerHasNeededSpearsLog;
        }

        // Save data goes into this class
        public class StarvingTownNeedsFoodIssueTypeDefiner : SaveableTypeDefiner
        {
            public StarvingTownNeedsFoodIssueTypeDefiner() : base(585850)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodIssue), 1);
                base.AddClassDefinition(typeof(StarvingTownNeedsFoodIssueBehavior.StarvingTownNeedsFoodQuest), 2);
            }
        }

        // Register this event to check for issue event
        public override void RegisterEvents()
        {
            CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnCheckForIssue));
        }

        // Unused Sync Data method?
        public override void SyncData(IDataStore dataStore)
        {
        }

    }
}
