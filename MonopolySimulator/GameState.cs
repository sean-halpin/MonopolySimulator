using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MonopolySimulator
{
    internal class GameState
    {
        private readonly List<Player> _players;
        private readonly List<Position> _positions;
        private readonly Random _rnd;
        private Player _activePlayer;
        private bool _gameFinished = false;

        public GameState(Random rnd, int playerCount, int startingCash)
        {
            _rnd = rnd;
            _players = PreparePlayers(startingCash, playerCount);
            _activePlayer = _players[0];
            _positions = JsonConvert.DeserializeObject<List<Position>>(File.ReadAllText("board.json"));
        }

        private static List<Player> PreparePlayers(int startingCash = 1200, int playerCount = 4)
        {
            return Enumerable.Range(0, playerCount).Select(e => new Player
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
            while (GameIsInProgress(_players))
            {
                var roll = new DiceRoll(_rnd).Roll();

                _activePlayer.Rolls.Enqueue(roll);
                _activePlayer.Position = (_activePlayer.Position + roll.TotalValue()) % _positions.Count;
                var currentPosition = _positions[_activePlayer.Position];

                switch (currentPosition.type)
                {
                    case PositionType.go:
                        _activePlayer.Balance += 200;
                        break;
                    case PositionType.property:
                        if (currentPosition.HasOwner())
                            _activePlayer.PayRent(currentPosition);
                        else
                            SimulateVacantPropertyLanding(currentPosition);
                        break;
                    case PositionType.communitychest:
                        break;
                    case PositionType.tax:
                        _activePlayer.Balance -= currentPosition.cost;
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
                        _activePlayer.Imprisoned = true;
                        _activePlayer.Position = _positions.IndexOf(_positions.First(p => p.name == Name.Jail));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (_activePlayer.Balance < 0)
                    _activePlayer.PlayerIsAlive = false;

                _activePlayer = GetNextPlayer(_activePlayer);
            }

            _players.ForEach(p =>
            {
                Console.WriteLine("Id:{0}\tBalance:{1}\tRolls:{2}\tProperties:{3}\tAlive:{4}",
                    p.Id,
                    p.Balance,
                    p.Rolls.Count,
                    p.PositionsAcquired.Count,
                    p.PlayerIsAlive);
            });
        }

        private bool GameIsInProgress(List<Player> players)
        {
            return players.Count(p => p.PlayerIsAlive) > 1;
        }

        private Player GetNextPlayer(Player activePlayer)
        {
            var remainingPlayers = _players.Where(player => player.PlayerIsAlive).ToList();
            return remainingPlayers[(remainingPlayers.IndexOf(activePlayer) + 1) % remainingPlayers.Count];
        }

        private void SimulateVacantPropertyLanding(Position currentPosition)
        {
            if (_activePlayer.WantsToPurchase(currentPosition))
            {
                if (_activePlayer.Balance >= currentPosition.cost)
                {
                    _activePlayer.PositionsAcquired.Add(currentPosition);
                    currentPosition.owner = _activePlayer;
                }
            }
            else
            {
                SimulateAuction(currentPosition);
            }
        }

        private void SimulateAuction(Position currentPosition)
        {
            //throw new NotImplementedException();
        }
    }
}