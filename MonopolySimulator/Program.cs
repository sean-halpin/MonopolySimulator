using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MonopolySimulator
{
    internal class Player
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public Queue<DiceRoll> Rolls { get; set; }
        public List<Position> PositionsAcquired { get; set; }
        public int Balance { get; set; }
        public bool Imprisoned { get; set; }
        public bool PlayerIsAlive { get; set; }
    }

    internal class DiceRoll
    {
        private readonly Random _rnd;
        private int _dieValue1;
        private int _dieValue2;

        public DiceRoll(Random rnd)
        {
            _rnd = rnd;
        }

        public DiceRoll Roll()
        {
            _dieValue1 = _rnd.Next(1, 6);
            _dieValue2 = _rnd.Next(1, 6);
            return this;
        }

        public int TotalValue()
        {
            return _dieValue1 + _dieValue2;
        }
    }

    internal class GameState
    {
        private int _activePlayer = 0;
        private Player _currentPlayer;
        private readonly Random _rnd;
        private readonly int _startingCash;
        private readonly List<Player> _players;
        private readonly List<Position> _positions;
        private bool _gameFinished = false;
        private readonly int _startingPlayerCount;

        public GameState(Random rnd, int playerCount, int startingCash)
        {
            _rnd = rnd;
            _startingPlayerCount = playerCount;
            _startingCash = startingCash;
            _players = PreparePlayers(_startingCash, playerCount);
            _positions = JsonConvert.DeserializeObject<List<Position>>(File.ReadAllText("board.json"));
        }

        private static List<Player> PreparePlayers(int startingCash = 1200, int playerCount = 4)
        {
            return Enumerable.Range(0, playerCount).Select(e => new Player()
            {
                Id = e,
                Balance = startingCash,
                PositionsAcquired = new List<Position>(),
                Position = 0,
                Imprisoned = false,
                PlayerIsAlive = true,
                Rolls = new Queue<DiceRoll>(3)
            }).ToList();
        }

        public void RunSimulation()
        {
            while (!_gameFinished)
            {
                _currentPlayer = _players[_activePlayer];
                if (_currentPlayer.PlayerIsAlive)
                {
                    var roll = new DiceRoll(_rnd).Roll();

                    _currentPlayer.Rolls.Enqueue(roll);
                    _currentPlayer.Position = (_currentPlayer.Position + roll.TotalValue()) % _positions.Count;
                    var currentPosition = _positions[_currentPlayer.Position];

                    switch (currentPosition.type)
                    {
                        case PositionType.go:
                            _currentPlayer.Balance += 200;
                            break;
                        case PositionType.property:
                            if (currentPosition.owner == null)
                            {
                                if (_currentPlayer.Balance >= currentPosition.cost)
                                {
                                    _currentPlayer.PositionsAcquired.Add(currentPosition);
                                    currentPosition.owner = _currentPlayer;
                                }
                            }
                            else
                            {
                                var debt = currentPosition.rent[0];
                                _currentPlayer.Balance -= debt;
                                currentPosition.owner.Balance += debt;
                            }
                            break;
                        case PositionType.communitychest:
                            break;
                        case PositionType.tax:
                            _currentPlayer.Balance -= currentPosition.cost;
                            break;
                        case PositionType.railroad:
                            break;
                        case PositionType.chance:
                            break;
                        case PositionType.jail:
                            break;
                        case PositionType.utility:
                            break;
                        case PositionType.freeparking:
                            break;
                        case PositionType.gotojail:
                            _currentPlayer.Imprisoned = true;
                            _currentPlayer.Position = _positions.IndexOf(_positions.First(p => p.name == Name.Jail));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (_currentPlayer.Balance < 0)
                        _currentPlayer.PlayerIsAlive = false;

                    if (_players.Count(p => p.PlayerIsAlive) <= 1)
                        _gameFinished = true;
                }
                _activePlayer = ++_activePlayer % _startingPlayerCount;
            }
        }
    }

    internal enum CommunityChestCards
    {
        Move_Advance_to_Go,
        Move_Go_back_to_Old_Kent_Road,
        Move_Go_to_jail_Move_directly_to_jail_Do_not_pass_Go_Do_not_collect_200,
        Tax_Fine_Pay_hospital_100,
        Tax_Fine_Doctors_fee_Pay_50,
        Tax_Fine_Pay_your_insurance_premium_50,
        Receipt_Bank_error_in_your_favour_Collect_200,
        Receipt_Annuity_matures_Collect_100,
        Receipt_You_inherit_100,
        Receipt_From_sale_of_stock_you_get_50,
        Receipt_Receive_interest_on_7percent_preference_shares_25,
        Receipt_Income_tax_refund_Collect_20,
        Receipt_You_have_won_second_prize_in_a_beauty_contest_Collect_10,
        Receipt_It_is_your_birthday_Collect_10_from_each_player,
        Other_Get_out_of_jail_free_This_card_may_be_kept_until_needed_or_sold,
        Other_Pay_a_10_fine_or_take_a_Chance
    }

    internal enum ChanceCards
    {
        Movement_Advance_to_Go,
        Movement_Go_to_jail_Move_directly_to_jail_Do_not_pass_Go_Do_not_collect_200,
        Movement_Advance_to_Pall_Mall_If_you_pass_Go_collection_200,
        Movement_Take_a_trip_to_Marylebone_Station_and_if_you_pass_Go_collect_200,
        Movement_Advance_to_Trafalgar_Square_If_you_pass_Go_collect_200,
        Movement_Advance_to_Mayfair,
        Movement_Go_back_three_spaces,
        Taxes_Fines_Make_general_repairs_on_all_of_your_houses_For_each_house_pay_25_For_each_hotel_pay_100,
        Taxes_Fines_You_are_assessed_for_street_repairs_40_per_house_115_per_hotel,
        Taxes_Fines_Pay_school_fees_of_150,
        Taxes_Fines_Drunk_in_charge_fine_20,
        Taxes_Fines_Speeding_fine_15,
        Receipts_Your_building_loan_matures_Receive_150,
        Receipts_You_have_won_a_crossword_competition_Collect_100,
        Receipts_Bank_pays_you_dividend_of_50,
        Other_Get_out_of_jail_free_This_card_may_be_kept_until_needed_or_sold
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            new GameState(new Random(), 4, 1200).RunSimulation();
        }
    }
}

//internal enum PositionNameUK
//{
//    Go = 0,
//    Old_Kent_Road,
//    Community_Chest_1,
//    Whitechapel_Road,
//    Income_Tax,
//    Kings_Cross_Station,
//    The_Angel_Islington,
//    Chance_1,
//    Euston_Road,
//    Pentonville_Road,
//    Jail,
//    Pall_Mall,
//    Electric_Company,
//    Whitehall,
//    Northumberland_Avenue,
//    Marylebone_Station,
//    Bow_Street,
//    Community_Chest_2,
//    Marlborough_Street,
//    Vine_Street,
//    Free_Parking,
//    Strand,
//    Chance_2,
//    Fleet_Street,
//    Trafalgar_Square,
//    Fenchurch_Street_Station,
//    Leicester_Square,
//    Coventry_Street,
//    Water_Works,
//    Piccadilly,
//    Go_To_Jail,
//    Regent_Street,
//    Oxford_Street,
//    Community_Chest_3,
//    Bond_Street,
//    Liverpool_Street_Station,
//    Chance_3,
//    Park_Lane,
//    Super_Tax,
//    Mayfair
//}