using System;
using System.Collections.Generic;
using System.Linq;
using MonopolySimulator.Enums;
using MonopolySimulator.Utils;

namespace MonopolySimulator.DomainModel
{
    internal class ChanceEngine
    {
        private readonly Random _random;
        private int _cardIndexPointer;
        private List<ChanceCards> _cards;

        public ChanceEngine(Random random)
        {
            _random = random;
            _cardIndexPointer = 0;
        }

        public void Simulate(Player activePlayer)
        {
            var currentCard = _cards[_cardIndexPointer];
            switch (currentCard)
            {
                case ChanceCards.Movement_Advance_to_Go:
                    activePlayer.MoveToPositionByName(Name.Go);
                    break;
                case ChanceCards.Movement_Go_to_jail_Move_directly_to_jail_Do_not_pass_Go_Do_not_collect_200:
                    activePlayer.Imprison();
                    break;
                case ChanceCards.Movement_Advance_to_St_Charles_Place_If_you_pass_Go_collection_200:
                    activePlayer.MoveToPositionByName(Name.StCharlesPlace);
                    break;
                case ChanceCards.Movement_Take_a_trip_to_Pennsylvania_Railroad_and_if_you_pass_Go_collect_200:
                    activePlayer.MoveToPositionByName(Name.PennsylvaniaRailroad);
                    break;
                case ChanceCards.Movement_Advance_to_IllnoisAvenue_If_you_pass_Go_collect_200:
                    activePlayer.MoveToPositionByName(Name.IllnoisAvenue);
                    break;
                case ChanceCards.Movement_Advance_to_Boardwalk:
                    activePlayer.MoveToPositionByName(Name.Boardwalk);
                    break;
                case ChanceCards.Movement_Go_back_three_spaces:
                    activePlayer.MoveBackward(3);
                    break;
                case ChanceCards.Taxes_Fines_Make_general_repairs_on_all_of_your_houses_For_each_house_pay_25_For_each_hotel_pay_100:
                    var repairsOnHouses = activePlayer.NumberOfHousesBought() * 25;
                    var repairsOnHotels = activePlayer.NumberOfHotelsBought() * 100;
                    activePlayer.DecreaseBalance(repairsOnHouses + repairsOnHotels);
                    break;
                case ChanceCards.Taxes_Fines_You_are_assessed_for_street_repairs_40_per_house_115_per_hotel:
                    var streetRepairsOnHouses = activePlayer.NumberOfHousesBought() * 40;
                    var streetRepairsOnHotels = activePlayer.NumberOfHotelsBought() * 115;
                    activePlayer.DecreaseBalance(streetRepairsOnHouses + streetRepairsOnHotels);
                    break;
                case ChanceCards.Taxes_Fines_Pay_school_fees_of_150:
                    activePlayer.DecreaseBalance(150);
                    break;
                case ChanceCards.Taxes_Fines_Drunk_in_charge_fine_20:
                    activePlayer.DecreaseBalance(20);
                    break;
                case ChanceCards.Taxes_Fines_Speeding_fine_15:
                    activePlayer.DecreaseBalance(15);
                    break;
                case ChanceCards.Receipts_Your_building_loan_matures_Receive_150:
                    activePlayer.IncreaseBalance(150);
                    break;
                case ChanceCards.Receipts_You_have_won_a_crossword_competition_Collect_100:
                    activePlayer.IncreaseBalance(100);
                    break;
                case ChanceCards.Receipts_Bank_pays_you_dividend_of_50:
                    activePlayer.IncreaseBalance(50);
                    break;
                case ChanceCards.Other_Get_out_of_jail_free_This_card_may_be_kept_until_needed_or_sold:
                    activePlayer.RecieveGetOutOfJailFreeCard();
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
            _cards = Enum.GetValues(typeof(ChanceCards)).Cast<ChanceCards>().ToList();
            _cards.Shuffle(_random);
        }
    }
}