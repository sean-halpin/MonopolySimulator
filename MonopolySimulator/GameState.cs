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
                        SimulateLandOnTax(_activePlayer, currentPosition);
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

        private void SimulateLandOnTax(Player activePlayer, Position currentPosition)
        {
            activePlayer.DecreaseBalance(currentPosition.cost);
            if (activePlayer.HasBeenBankrupted())
                SimulateBankruptcyByBanker(activePlayer);
        }

        private void SimulateBankruptcyByBanker(Player activePlayer)
        {
            activePlayer.KillPlayer();
            foreach (var property in activePlayer.PositionsAcquired)
            {
                property.buildingCount = 0;
                property.owner = null;
            }
            var propertiesToBeAuctionedImmediately = activePlayer.PositionsAcquired;
        }

        private void SimulatePassingGo(Player activePlayer)
        {
            activePlayer.IncreaseBalance(200);
        }

        private bool GameInProgress(List<Player> players)
        {
            return players.Count(p => p.PlayerIsAlive) > 1;
        }

        private Player GetNextPlayer(Player activePlayer)
        {
            var remainingPlayers = _players.Where(player => player.PlayerIsAlive || player.Id == activePlayer.Id)
                .ToList();
            return remainingPlayers[(remainingPlayers.IndexOf(activePlayer) + 1) % remainingPlayers.Count];
        }

        private void SimulateLandOnVacantProperty(Player activePlayer, Position currentPosition)
        {
            if (currentPosition.HasOwner())
            {
                activePlayer.PayRent(currentPosition);
                if (activePlayer.HasBeenBankrupted())
                    SimulateBankruptcyByPlayer(activePlayer, currentPosition.owner);
            }
            else
            {
                if (activePlayer.WantsToPurchase(currentPosition))
                {
                    if (activePlayer.Balance >= currentPosition.cost)
                    {
                        activePlayer.PositionsAcquired.Add(currentPosition);
                        currentPosition.owner = activePlayer;
                    }
                }
                else
                {
                    SimulateAuction(currentPosition);
                }
            }
        }

        private void SimulateBankruptcyByPlayer(Player activePlayer, Player currentPositionOwner)
        {
            activePlayer.KillPlayer();
            foreach (var property in activePlayer.PositionsAcquired)
            {
                property.buildingCount = 0;
                property.owner = currentPositionOwner;
            }
        }

        private void SimulateAuction(Position currentPosition)
        {
            //throw new NotImplementedException();
        }
    }
}