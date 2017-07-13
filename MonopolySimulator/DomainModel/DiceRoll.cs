using System;

namespace MonopolySimulator.DomainModel
{
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

        public bool IsADouble()
        {
            return _dieValue1 == _dieValue2;
        }
    }
}