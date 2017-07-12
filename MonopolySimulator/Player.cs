using System.Collections.Generic;

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

        public void PayRent(Position currentPosition)
        {
            var rentDue = currentPosition.rent[currentPosition.buildingCount];
            Balance -= rentDue;
            currentPosition.owner.Balance += rentDue;
        }

        public bool WantsToPurchase(Position currentPosition)
        {
            return true; //Wants to buy all atm. 
        }
    }
}