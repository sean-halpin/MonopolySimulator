using System;
using System.Collections.Generic;

namespace MonopolySimulator
{
    internal class Player
    {
        private Player(int id, int balance)
        {
            Id = id;
            Balance = balance;
            Rolls = new Queue<DiceRoll>();
            PositionsAcquired = new List<Position>();
            Imprisoned = false;
            PlayerIsAlive = true;
        }

        public int Id { get; }
        public int Position { get; set; }
        public Queue<DiceRoll> Rolls { get; set; }
        public List<Position> PositionsAcquired { get; set; }
        public int Balance { get; private set; }
        public bool Imprisoned { get; set; }
        public bool PlayerIsAlive { get; set; }

        public void PayRent(Position currentPosition)
        {
            var rentDue = currentPosition.rent[currentPosition.buildingCount];
            DecreaseBalance(rentDue);
            currentPosition.owner.IncreaseBalance(rentDue);
        }

        public bool WantsToPurchase(Position currentPosition)
        {
            return true; //Wants to buy all atm. 
        }

        public void Roll(Random rnd)
        {
            var roll = new DiceRoll(rnd).Roll();
            Rolls.Enqueue(roll);
            Position = (Position + roll.TotalValue()) % 40;
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
    }
}