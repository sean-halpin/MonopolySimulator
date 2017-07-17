using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonopolySimulator.Enums;
using Newtonsoft.Json;

namespace MonopolySimulator.DomainModel
{
    internal class GameEngine
    {
        private readonly ChanceEngine _chanceEngine;
        private readonly CommunityChestEngine _communityChestEngine;
        private readonly List<Player> _players;
        private readonly List<Position> _positions;
        private readonly Random _random;
        private Player _activePlayer;

        public GameEngine(Random random, CommunityChestEngine communityChestEngine, ChanceEngine chanceEngine,
            int playerCount, int startingBalance)
        {
            _random = random;
            _communityChestEngine = communityChestEngine;
            _chanceEngine = chanceEngine;
            _players = PreparePlayers(playerCount, startingBalance);
            _activePlayer = _players[0];
            _positions = JsonConvert.DeserializeObject<List<Position>>(File.ReadAllText(@"BoardTemplate\board.json"));
        }

        private static List<Player> PreparePlayers(int playerCount, int startingBalance)
        {
            return Enumerable.Range(0, playerCount).Select(e => Player.CreateNew(e, startingBalance)).ToList();
        }

        public void RunSimulation()
        {
            while (GameInProgress(_players))
            {
                _activePlayer.RollAndUpdatePosition(_random);
                var currentPosition = _positions[_activePlayer.PositionIndex];

                switch (currentPosition.type)
                {
                    case PositionType.go:
                        break;
                    case PositionType.property:
                        SimulateLandOnProperty(_activePlayer, currentPosition);
                        break;
                    case PositionType.communitychest:
                        SimulateCommunityChest(_activePlayer, _players);
                        break;
                    case PositionType.tax:
                        SimulateLandOnTax(_activePlayer, currentPosition);
                        break;
                    case PositionType.railroad:
                        break;
                    case PositionType.chance:
                        SimulateLandOnChance(_activePlayer);
                        break;
                    case PositionType.jail:
                        SimulateLandOnJail(_activePlayer, _random);
                        break;
                    case PositionType.utility:
                        break;
                    case PositionType.freeparking:
                        break;
                    case PositionType.gotojail:
                        SimulateLandOnGoToJail(_activePlayer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SimulatePlayerRealEstateDecision(_activePlayer);

                _activePlayer = GetNextPlayer(_activePlayer);
            }

            _players.ForEach(p =>
            {
                Console.WriteLine("Id:{0}\tBalance:{1}\tRolls:{2}\tPositions:{3}\tAlive:{4}\tHouses{5}",
                    p.Id,
                    p.Balance,
                    p.Rolls.Count,
                    p.PositionsAcquired.Count,
                    p.PlayerIsAlive,
                    p.PositionsAcquired.Sum(pos => pos.BuildingCount));
            });
        }

        private void SimulateLandOnChance(Player activePlayer)
        {
            _chanceEngine.Simulate(activePlayer);
            if (activePlayer.HasBeenBankrupted())
                SimulateBankruptcyByBanker(activePlayer);
        }

        private void SimulateCommunityChest(Player activePlayer, List<Player> players)
        {
            _communityChestEngine.Simulate(activePlayer, players);
            if (activePlayer.HasBeenBankrupted())
                SimulateBankruptcyByBanker(activePlayer);
        }

        private static void SimulateLandOnJail(Player activePlayer, Random random)
        {
            if (activePlayer.Imprisoned)
            {
                activePlayer.RollInPrison(random);
                if (activePlayer.ShouldBeReleasedFromPrison())
                    activePlayer.ReleaseFromPrison();
            }
        }

        private static void SimulateLandOnGoToJail(Player activePlayer)
        {
            activePlayer.Imprison();
        }

        private void SimulatePlayerRealEstateDecision(Player activePlayer)
        {
            var playersPositionsFormingMonopoliesForPlayer = GetPositionsFormingMonopoliesForPlayer(activePlayer);
            foreach (var position in playersPositionsFormingMonopoliesForPlayer)
            {
                if (!activePlayer.WantsToPurchaseBuildingAtPosition(position) ||
                    position.MaxBuildingsReached()) continue;
                activePlayer.DecreaseBalance(position.house);
                position.AddBuilding(1);
            }
        }

        private IEnumerable<Position> GetPositionsFormingMonopoliesForPlayer(Player activePlayer)
        {
            return _activePlayer.PositionsAcquired.Where(p => p.type == PositionType.property)
                .Where(property => _positions.Where(p => p.group != null && p.group[0] == property.group[0])
                    .All(p => p.owner != null && p.owner.Id == activePlayer.Id))
                .ToList();
        }

        private void SimulateLandOnTax(Player activePlayer, Position currentPosition)
        {
            activePlayer.DecreaseBalance(currentPosition.cost);
            if (activePlayer.HasBeenBankrupted())
                SimulateBankruptcyByBanker(activePlayer);
        }

        private void SimulateBankruptcyByBanker(Player activePlayer)
        {
            foreach (var property in activePlayer.PositionsAcquired)
            {
                property.DemolishBuildings();
                property.Repossess();
            }
            var propertiesToBeAuctionedImmediately = activePlayer.PositionsAcquired;
            foreach (var property in propertiesToBeAuctionedImmediately)
                SimulateAuction(property);
            activePlayer.KillPlayer();
        }

        private static void SimulatePassingGo(Player activePlayer)
        {
            activePlayer.IncreaseBalance(200);
        }

        private static bool GameInProgress(IEnumerable<Player> players)
        {
            return players.Count(p => p.PlayerIsAlive) > 1;
        }

        private Player GetNextPlayer(Player activePlayer)
        {
            var remainingPlayers = _players.Where(player => player.PlayerIsAlive || player.Id == activePlayer.Id)
                .ToList();
            return remainingPlayers[(remainingPlayers.IndexOf(activePlayer) + 1) % remainingPlayers.Count];
        }

        private void SimulateLandOnProperty(Player activePlayer, Position currentPosition)
        {
            if (currentPosition.HasOwner())
            {
                activePlayer.PayRent(currentPosition);
                if (activePlayer.HasBeenBankrupted())
                    SimulateBankruptcyByPlayer(activePlayer, currentPosition.owner);
            }
            else
            {
                if (activePlayer.WantsToPurchasePosition(currentPosition))
                    activePlayer.PurchasePosition(currentPosition);
                else
                    SimulateAuction(currentPosition);
            }
        }

        private static void SimulateBankruptcyByPlayer(Player activePlayer, Player currentPositionOwner)
        {
            foreach (var property in activePlayer.PositionsAcquired)
            {
                property.DemolishBuildings();
                property.AssignNewOwner(currentPositionOwner);
            }
            activePlayer.KillPlayer();
        }

        private void SimulateAuction(Position currentPosition)
        {
            //throw new NotImplementedException();
        }
    }
}