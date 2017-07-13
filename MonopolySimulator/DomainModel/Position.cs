using System.Collections.Generic;
using MonopolySimulator.Enums;

namespace MonopolySimulator.DomainModel
{
    internal class Position
    {
        public int BuildingCount { get; private set; }

        public Name name { get; set; }
        public PositionType type { get; set; }
        public Player owner { get; private set; }
        public bool corner { get; set; }
        public int cost { get; set; }
        public string color { get; set; }
        public List<int> rent { get; set; }
        public List<int> group { get; set; }
        public int house { get; set; }

        public bool HasOwner()
        {
            return owner != null;
        }

        public bool MaxBuildingsReached()
        {
            return BuildingCount >= 5;
        }

        public void AddBuilding(int i)
        {
            BuildingCount += 1;
        }

        public void DemolishBuildings()
        {
            BuildingCount = 0;
        }

        public void Repossess()
        {
            owner = null;
        }

        public void AssignNewOwner(Player activePlayer)
        {
            owner = activePlayer;
        }
    }
}