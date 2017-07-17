using System;
using System.Collections.Generic;
using System.Linq;
using MonopolySimulator.Enums;
using MonopolySimulator.Utils;

namespace MonopolySimulator.DomainModel
{
    internal class CommunityChestEngine
    {
        private readonly Random _random;
        private List<CommunityChestCards> _cards;
        private int _cardIndexPointer;

        public CommunityChestEngine(Random random)
        {
            _random = random;
            _cardIndexPointer = 0;
        }

        public void Simulate(Player activePlayer, List<Player> players)
        {
            var currentCard = _cards[_cardIndexPointer];
            switch (currentCard)
            {
                case CommunityChestCards.Move_Advance_to_Go:
                    activePlayer.MoveToPositionByName(Name.Go);
                    activePlayer.IncreaseBalance(200);
                    break;
                case CommunityChestCards.Move_Go_back_to_Mediterranean_Avenue:
                    activePlayer.MoveToPositionByName(Name.MediterraneanAvenue);
                    break;
                case CommunityChestCards.Move_Go_to_jail_Move_directly_to_jail_Do_not_pass_Go_Do_not_collect_200:
                    activePlayer.Imprison();
                    break;
                case CommunityChestCards.Tax_Fine_Pay_hospital_100:
                    activePlayer.DecreaseBalance(100);
                    break;
                case CommunityChestCards.Tax_Fine_Doctors_fee_Pay_50:
                    activePlayer.DecreaseBalance(50);
                    break;
                case CommunityChestCards.Tax_Fine_Pay_your_insurance_premium_50:
                    activePlayer.DecreaseBalance(50);
                    break;
                case CommunityChestCards.Receipt_Bank_error_in_your_favour_Collect_200:
                    activePlayer.IncreaseBalance(200);
                    break;
                case CommunityChestCards.Receipt_Annuity_matures_Collect_100:
                    activePlayer.IncreaseBalance(200);
                    break;
                case CommunityChestCards.Receipt_You_inherit_100:
                    activePlayer.IncreaseBalance(100);
                    break;
                case CommunityChestCards.Receipt_From_sale_of_stock_you_get_50:
                    activePlayer.IncreaseBalance(50);
                    break;
                case CommunityChestCards.Receipt_Receive_interest_on_7percent_preference_shares_25:
                    activePlayer.IncreaseBalance(25);
                    break;
                case CommunityChestCards.Receipt_Income_tax_refund_Collect_20:
                    activePlayer.IncreaseBalance(20);
                    break;
                case CommunityChestCards.Receipt_You_have_won_second_prize_in_a_beauty_contest_Collect_10:
                    activePlayer.IncreaseBalance(10);
                    break;
                case CommunityChestCards.Receipt_It_is_your_birthday_Collect_10_from_each_player:
                    foreach (var player in players.Where(p => p.PlayerIsAlive && p.Id != activePlayer.Id))
                    {
                        player.PayDebt(activePlayer, 10);
                    }
                    break;
                case CommunityChestCards.Other_Get_out_of_jail_free_This_card_may_be_kept_until_needed_or_sold:
                    activePlayer.RecieveGetOutOfJailFreeCard();
                    break;
                case CommunityChestCards.Other_Pay_a_10_fine_or_take_a_Chance:
                    activePlayer.DecreaseBalance(10);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            MovePointer();
        }

        private void MovePointer()
        {
            _cardIndexPointer = (_cardIndexPointer + 1) % _cards.Count;
        }

        public void Initialise()
        {
            _cards = Enum.GetValues(typeof(CommunityChestCards)).Cast<CommunityChestCards>().ToList();
            _cards.Shuffle(_random);
        }
    }
}