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
        public List<Position> PositionsAcquired { get; set; }
        public int Balance { get; private set; }
        public bool Imprisoned { get; private set; }
        public bool PlayerIsAlive { get; private set; }

        public void PayRent(Position currentPosition)
        {
            var rentDue = currentPosition.rent[currentPosition.BuildingCount];
            DecreaseBalance(rentDue);
            currentPosition.owner.IncreaseBalance(rentDue);
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
            PositionIndex = (PositionIndex + rollValue) % 40;
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
            Balance -= amount;
        }

        public bool HasBeenBankrupted()
        {
            return Balance < 0;
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
            return Balance - amount >= 0;
        }

        public void PurchasePosition(Position currentPosition)
        {
            PositionsAcquired.Add(currentPosition);
            currentPosition.AssignNewOwner(this);
        }

        public void Imprison()
        {
            Imprisoned = true;
            MoveToPositionIndex((int) Name.Jail);
        }

        public void MoveToPositionIndex(int positionIndex)
        {
            PositionIndex = positionIndex;
        }

        public bool ShouldBeReleasedFromPrison()
        {
            return RollsInPrison.Count == 3 || RollsInPrison.Last().IsADouble();
        }

        public void ReleaseFromPrison()
        {
            Imprisoned = false;
            MoveForward(RollsInPrison.Last().TotalValue());
            RollsInPrison.Clear();
        }
    }
}