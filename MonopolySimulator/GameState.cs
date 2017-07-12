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
        private readonly Random _random;
        private Player _activePlayer;

        public GameState(Random random, int playerCount, int startingBalance)
        {
            _random = random;
            _players = PreparePlayers(playerCount, startingBalance);
            _activePlayer = _players[0];
            _positions = JsonConvert.DeserializeObject<List<Position>>(File.ReadAllText("board.json"));
        }

        private static List<Player> PreparePlayers(int playerCount, int startingBalance)
        {
            return Enumerable.Range(0, playerCount).Select(e => Player.CreateNew(e, startingBalance)).ToList();
        }

        public void RunSimulation()
        {
            while (GameInProgress(_players))
            {
                _activePlayer.Roll(_random);
                var currentPosition = _positions[_activePlayer.Position];

                switch (currentPosition.type)
                {
                    case PositionType.go:
                        SimulatePassingGo(_activePlayer);
                        break;
                    case PositionType.property:
                        SimulateLandOnVacantProperty(_activePlayer, currentPosition);
                        break;
                    case PositionType.communitychest:
                        break;
                    case PositionType.tax:
                        _activePlayer.DecreaseBalance(currentPosition.cost);
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

        private void SimulatePassingGo(Player activePlayer) => activePlayer.IncreaseBalance(200);

        private bool GameInProgress(List<Player> players) => players.Count(p => p.PlayerIsAlive) > 1;

        private Player GetNextPlayer(Player activePlayer)
        {
            var remainingPlayers = _players.Where(player => player.PlayerIsAlive).ToList();
            return remainingPlayers[(remainingPlayers.IndexOf(activePlayer) + 1) % remainingPlayers.Count];
        }

        private void SimulateLandOnVacantProperty(Player activeplayer, Position currentPosition)
        {
            if (currentPosition.HasOwner())
                _activePlayer.PayRent(currentPosition);
            else
            {
                if (activeplayer.WantsToPurchase(currentPosition))
                {
                    if (activeplayer.Balance >= currentPosition.cost)
                    {
                        activeplayer.PositionsAcquired.Add(currentPosition);
                        currentPosition.owner = activeplayer;
                    }
                }
                else
                {
                    SimulateAuction(currentPosition);
                }
            }
        }

        private void SimulateAuction(Position currentPosition)
        {
            //throw new NotImplementedException();
        }
    }
}