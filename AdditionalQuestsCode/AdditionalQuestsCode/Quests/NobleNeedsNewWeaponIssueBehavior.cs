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
    public class NobleNeedsNewWeaponIssueBehavior : CampaignBehaviorBase
    {
        // Needs to be a noble
        private bool ConditionsHold(Hero issueGiver)
        {
            if (!issueGiver.IsFactionLeader)
            {
                Clan clan = issueGiver.Clan;
                if (((clan != null) ? clan.Leader : null) != issueGiver)
                {
                    return false;
                }
            }
            if (!issueGiver.IsMinorFactionHero && issueGiver.Clan != Clan.PlayerClan)
            {
                MobileParty partyBelongedTo = issueGiver.PartyBelongedTo;
                if (partyBelongedTo != null)
                {
                    return true;
                }
            }
            return false;
        }

        // If the conditions hold, start this quest, otherwise just add it as a possible quest
        public void OnCheckForIssue(Hero hero)
        {
            if (this.ConditionsHold(hero))
            {
                Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnIssueSelected), typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Rare));
                return;
            }
            Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(VillageBanditArmyRaidIssueBehavior.VillageBanditArmyRaidIssue), IssueBase.IssueFrequency.Rare));
        }

        private IssueBase OnIssueSelected(in PotentialIssueData pid, Hero issueOwner)
        {
            return new NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponIssue(issueOwner);
        }

        // Now the Issue
        internal class NobleNeedsNewWeaponIssue : IssueBase
        {
            public NobleNeedsNewWeaponIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(20f))
            {
                WeaponTypeForQuest = issueOwner.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.WeaponItemBeginSlot).Item.WeaponComponent.Weapons[0].WeaponClass;
            }

            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} needs new weapon", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueOwner.Name);
                    return textObject;
                }
            }

            public override TextObject Description
            {
                get
                {
                    TextObject textObject = new TextObject("{NOBLE_NAME} wants to have a finely crafted weapon made for them, to replace their old one", null);
                    textObject.SetTextVariable("NOBLE_NAME", base.IssueSettlement.Name);
                    return textObject;
                }
            }

            public override TextObject IssueBriefByIssueGiver
            {
                get
                {
                    return new TextObject("My weapon of choice was broken in a recent battle and there seems to be little hope of fixing it anew. In the meantime I've decided to allow some merchants and smithys to provide me with a suitable replacement.", null);
                }
            }

            public override TextObject IssueAcceptByPlayer
            {
                get
                {
                    return new TextObject("Have you found a good weapon yet?", null);
                }
            }

            public override TextObject IssueQuestSolutionExplanationByIssueGiver
            {
                get
                {
                    TextObject textObject = new TextObject("No, I have not. If you find a {WEAPON_TYPE} of at least value 3000{GOLD_ICON} in your travels, please bring it to me, I will happily buy it from you for double the market rate. If you feel up to the challenge, you could even try smithing it instead.", null);
                    textObject.SetTextVariable("WEAPON_TYPE", WeaponTypeForQuest.ToString());
                    return textObject;
                }
            }

            public override TextObject IssueQuestSolutionAcceptByPlayer
            {
                get
                {
                    return new TextObject("I will find you a new weapon.", null);
                }
            }

            public override TextObject IssueAsRumorInSettlement
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.NAME} has been looking for a new weapon to replace their current one. I heard they broke it cutting right through both a shield and the solider behind it, all in one blow!", null);
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
                return IssueBase.IssueFrequency.Rare;
            }

            public override bool IssueStayAliveConditions()
            {
                return true;
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
                return new NobleNeedsNewWeaponQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(20f), this.RewardGold, WeaponTypeForQuest, 3000);
            }

            protected override void OnGameLoad()
            {
            }

            private readonly WeaponClass WeaponTypeForQuest;
        }

        internal class NobleNeedsNewWeaponQuest : QuestBase
        {
            // Constructor with basic vars and any vars about the quest
            public NobleNeedsNewWeaponQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold, WeaponClass weaponTypeForQuest,int weaponGoldValue) : base(questId, questGiver, duration, rewardGold)
            {
                WeaponTypeForQuest = weaponTypeForQuest;
                WeaponGoldValue = weaponGoldValue;
            }


            // All of our text/logs
            public override TextObject Title
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK} wants a new {WEAPON_TYPE}", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("WEAPON_TYPE", WeaponTypeForQuest.ToString());
                    return textObject;
                }
            }

            private TextObject StageOnePlayerAcceptsQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{QUEST_GIVER.LINK} wants a new {WEAPON_TYPE}. {?QUEST_GIVER.GENDER}She{?}He{\\?} will pay you double the market price for the weapon, so long as the weapon is worth more then 3000 gold. \n You can either buy or smith a {WEAPON_TYPE} to hand in.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    textObject.SetTextVariable("WEAPON_TYPE", WeaponTypeForQuest.ToString());
                    return textObject;
                }
            }

            private TextObject StageTwoPlayerHasWeaponText
            {
                get
                {
                    TextObject textObject = new TextObject("You now have a weapon to complete the quest. Return to {QUEST_GIVER.LINK} to hand it over.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageTimeoutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have failed to find a weapon for {QUEST_GIVER.LINK}, {?QUEST_GIVER.GENDER}she{?}he{\\?} is disapointed.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageSuccessLogText
            {
                get
                {
                    TextObject textObject = new TextObject("You have delivered a weapon to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is impressed by it and will now use it in battle!.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject);
                    return textObject;
                }
            }

            private TextObject StageCancelDueToWarLogText
            {
                get
                {
                    TextObject textObject = new TextObject("Your clan is now at war with {ISSUE_GIVER.LINK}'s faction. Your agreement with {ISSUE_GIVER.LINK} was canceled.", null);
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
                CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
            }

            private void OnPlayerInventoryExchange(List<ValueTuple<ItemRosterElement, int>> purchasedItems, List<ValueTuple<ItemRosterElement, int>> soldItems, bool isTrading)
            {
                bool flag = false;
                foreach (ValueTuple<ItemRosterElement, int> valueTuple in purchasedItems)
                {
                    ItemRosterElement item = valueTuple.Item1;
                    if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponTypeForQuest && item.EquipmentElement.Item.Value >= WeaponGoldValue)
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
                        if (item.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponTypeForQuest && item.EquipmentElement.Item.Value >= WeaponGoldValue)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    this.CheckIfPlayerReadyToReturnWeapon();
                }
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
                TextObject thankYouText = new TextObject("Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}! This will be a fine weapon to fight with.", null);
                thankYouText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);
                TextObject waitingText = new TextObject("I await a a worthy weapon, {?PLAYER.GENDER}milady{?}sir{\\?}.", null);
                waitingText.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject);


                this.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(thankYouText, null, null).Condition(() => CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject).Consequence(new ConversationSentence.OnConsequenceDelegate(this.QuestAcceptedConsequences)).CloseDialog();
                this.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("Have you bought a worthy weapon?", null), null, null).Condition(delegate
                {
                    return CharacterObject.OneToOneConversationCharacter == base.QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("Yes. Here is is.", null), null).ClickableCondition(new ConversationSentence.OnClickableConditionDelegate(this.GiveWeaponClickableConditions)).NpcLine(thankYouText, null, null).Consequence(delegate
                {
                    Campaign.Current.ConversationManager.ConversationEndOneShot += this.Success;
                }).CloseDialog().PlayerOption(new TextObject("I'm working on it.", null), null).NpcLine(waitingText, null, null).CloseDialog().EndPlayerOptions().CloseDialog();
            }

            private bool GiveWeaponClickableConditions(out TextObject explanation)
            {
                if (GetRequiredWeaponOnPlayer())
                {
                    explanation = TextObject.Empty;
                    return true;
                }
                explanation = new TextObject("You don't have a weapon to give.", null);
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
                TextObject objectiveText = new TextObject("Find {WEAPON_TYPE} worth 3000{GOLD_ICON}", null);
                objectiveText.SetTextVariable("WEAPON_TYPE", WeaponTypeForQuest.ToString());
                this.PlayerAcceptedQuestLog = base.AddDiscreteLog(this.StageOnePlayerAcceptsQuestLogText, objectiveText, GetRequiredWeaponOnPlayer() ? 0 : 1, 1, null, false);
            }

            private bool GetRequiredWeaponOnPlayer()
            {
                foreach (ItemRosterElement itemRosterElement in PartyBase.MainParty.ItemRoster)
                {
                    if (itemRosterElement.EquipmentElement.Item != null && itemRosterElement.EquipmentElement.Item.WeaponComponent != null && itemRosterElement.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponTypeForQuest && itemRosterElement.EquipmentElement.Item.Value >= WeaponGoldValue)
                    {
                        return true;
                    }
                }
                return false;
            }

            private void CheckIfPlayerReadyToReturnWeapon()
            {
                if (this.PlayerHasNeededWeaponLog == null && GetRequiredWeaponOnPlayer())
                {
                    this.PlayerHasNeededWeaponLog = base.AddLog(this.StageTwoPlayerHasWeaponText, false);
                    return;
                }
                if (this.PlayerHasNeededWeaponLog != null && !GetRequiredWeaponOnPlayer())
                {
                    base.RemoveLog(this.PlayerHasNeededWeaponLog);
                    this.PlayerHasNeededWeaponLog = null;
                }
            }

            private void Success()
            {
                base.CompleteQuestWithSuccess();
                base.AddLog(this.StageSuccessLogText, false);

                // Find weapon to sell
                foreach (ItemRosterElement itemRosterElement in PartyBase.MainParty.ItemRoster)
                {
                    if (itemRosterElement.EquipmentElement.Item != null && itemRosterElement.EquipmentElement.Item.WeaponComponent != null && itemRosterElement.EquipmentElement.Item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponTypeForQuest && itemRosterElement.EquipmentElement.Item.Value >= WeaponGoldValue)
                    {
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, itemRosterElement.EquipmentElement.ItemValue*2, false);
                        PartyBase.MainParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -1);
                        this.RelationshipChangeWithQuestGiver = 10;
                        ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
                        break;
                    }
                }
            }

            private void Fail()
            {
                this.RelationshipChangeWithQuestGiver = -5;
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, true, true);
            }

            // Saved vars/logs
            [SaveableField(10)]
            private readonly WeaponClass WeaponTypeForQuest;

            [SaveableField(20)]
            private readonly int WeaponGoldValue;

            [SaveableField(30)]
            private JournalLog PlayerAcceptedQuestLog;

            [SaveableField(40)]
            private JournalLog PlayerHasNeededWeaponLog;
        }

        // Save data goes into this class
        public class NobleNeedsNewWeaponIssueTypeDefiner : SaveableTypeDefiner
        {
            public NobleNeedsNewWeaponIssueTypeDefiner() : base(585830)
            {
            }

            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponIssue), 1);
                base.AddClassDefinition(typeof(NobleNeedsNewWeaponIssueBehavior.NobleNeedsNewWeaponQuest), 2);
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
