using System;
using System.Collections.Generic;
using System.Linq;
using MonopolySimulator.Enums;

namespace MonopolySimulator.DomainModel
{
    internal class Player
    {
        private Player(int id, int balance)
        {
            Id = id;
            Balance = balance;
            Rolls = new List<DiceRoll>();
            RollsInPrison = new List<DiceRoll>();
            PositionsAcquired = new List<Position>();
            Imprisoned = false;
            PlayerIsAlive = true;
        }

        public int Id { get; }
        public int PositionIndex { get; private set; }
        public List<DiceRoll> Rolls { get; }
        public List<DiceRoll> RollsInPrison { get; }
        public List<Position> PositionsAcquired { get; }
        public int Balance { get; private set; }
        public bool Imprisoned { get; private set; }
        public bool PlayerIsAlive { get; private set; }

        public int GetOutOfJailFreeCardCount { get; private set; }

        public void PayRent(Position currentPosition)
        {
            var rentDue = 0;
            switch (currentPosition.type)
            {
                case PositionType.property:
                    rentDue = currentPosition.rent[currentPosition.BuildingCount];
                    break;
                case PositionType.railroad:
                    rentDue = currentPosition.owner.RailRoadRentDue();
                    break;
                case PositionType.utility:
                    rentDue = currentPosition.owner.UtilityRentDue(Rolls.Last().TotalValue());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            PayDebt(currentPosition.owner, rentDue);
        }

        private int UtilityRentDue(int diceRolleValue)
        {
            switch (NumberOfUtilitiesOwned())
            {
                case 1: return 4 * diceRolleValue;
                case 2: return 10 * diceRolleValue;
                default: return 0;
            };
        }

        private int NumberOfUtilitiesOwned()
        {
            return PositionsAcquired.Count(p => p.group[0] == 10);
        }

        private int RailRoadRentDue()
        {
            switch (NumberOfRailRoadsOwned())
            {
                case 1: return 25;
                case 2: return 50;
                case 3: return 100;
                case 4: return 200;
                default: return 0;
            }
        }

        private int NumberOfRailRoadsOwned()
        {
            return PositionsAcquired.Count(p => p.group[0] == 9);
        }

        public bool WantsToPurchasePosition(Position currentPosition)
        {
            return CanAffordExpense(currentPosition.cost) && true;
        }

        public void RollAndUpdatePosition(Random rnd)
        {
            if (!Imprisoned)
            {
                var roll = new DiceRoll(rnd).Roll();
                Rolls.Add(roll);
                if (LastThreeRollsWereDoubles())
                    Imprison();
                else
                    MoveForward(roll.TotalValue());
            }
        }

        private bool LastThreeRollsWereDoubles()
        {
            return Enumerable.Reverse(Rolls).Take(3).All(roll => roll.IsADouble());
        }

        public void MoveForward(int rollValue)
        {
            if (PositionIndex + rollValue >= 40)
                IncreaseBalance(200);
            PositionIndex = (PositionIndex + rollValue) % 40;
        }

        public void MoveBackward(int rollValue)
        {
            PositionIndex = (PositionIndex - rollValue) % 40;
        }

        public void RollInPrison(Random rnd)
        {
            var roll = new DiceRoll(rnd).Roll();
            RollsInPrison.Add(roll);
        }

        public static Player CreateNew(int id, int startingBalance)
        {
            return new Player(id, startingBalance);
        }

        public void IncreaseBalance(int amount)
        {
            Balance += amount;
        }

        public void DecreaseBalance(int amount)
        {
            if (amount > Balance)
            {
                Balance = 0;
            }
            else
            {
                Balance -= amount;
            }
        }

        public bool HasBeenBankrupted()
        {
            return Balance <= 0;
        }

        public void KillPlayer()
        {
            PlayerIsAlive = false;
        }

        public bool WantsToPurchaseBuildingAtPosition(Position property)
        {
            return CanAffordExpense(property.house) && property.BuildingCount <= 3;
        }

        public bool CanAffordExpense(int amount)
        {
            return Balance - amount > 0;
        }

        public void PurchasePosition(Position currentPosition)
        {
            PositionsAcquired.Add(currentPosition);
            currentPosition.AssignNewOwner(this);
        }

        public void Imprison()
        {
            Imprisoned = true;
            MoveToPositionByName(Name.Jail);
        }

        public void MoveToPositionByName(Name positionName)
        {
            PositionIndex = (int)positionName;
        }

        public bool ShouldBeReleasedFromPrison()
        {
            if (GetOutOfJailFreeCardCount > 0)
            {
                GetOutOfJailFreeCardCount -= 1;
                return true;
            }

            return RollsInPrison.Count == 3 || RollsInPrison.Last().IsADouble();
        }

        public void ReleaseFromPrison()
        {
            Imprisoned = false;
            MoveForward(RollsInPrison.Last().TotalValue());
            RollsInPrison.Clear();
        }

        public void PayDebt(Player player, int debtAmount)
        {
            if (debtAmount > Balance)
            {
                player.IncreaseBalance(Balance);
                DecreaseBalance(Balance);
            }
            else
            {
                DecreaseBalance(debtAmount);
                player.IncreaseBalance(debtAmount);
            }
        }

        public void RecieveGetOutOfJailFreeCard()
        {
            GetOutOfJailFreeCardCount += 1;
        }

        public int NumberOfHousesBought()
        {
            return PositionsAcquired.Sum(p => p.BuildingCount) - PositionsAcquired.Count(p => p.MaxBuildingsReached());
        }

        public int NumberOfHotelsBought()
        {
            return PositionsAcquired.Count(p => p.MaxBuildingsReached());
        }
    }
}