using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private Player _activePlayer;
        private int stepCount;

        public GameEngine(Random random, CommunityChestEngine communityChestEngine, ChanceEngine chanceEngine,
            int playerCount, int startingBalance)
        {
            _communityChestEngine = communityChestEngine;
            _chanceEngine = chanceEngine;
            _players = PreparePlayers(playerCount, startingBalance, random);
            _activePlayer = _players[0];
            _positions = JsonConvert.DeserializeObject<List<Position>>(File.ReadAllText(@"BoardTemplate\board.json"));
        }

        private List<Player> PreparePlayers(int playerCount, int startingBalance, Random random)
        {
            return Enumerable.Range(0, playerCount).Select(e => new Player(e, startingBalance, random)).ToList();
        }

        public void RunSimulation()
        {
            while (GameInProgress(_players))
            {
                _activePlayer.RollAndUpdatePosition();
                var currentPosition = _positions[_activePlayer.PositionIndex];

                switch (currentPosition.type)
                {
                    case PositionType.property:
                    case PositionType.railroad:
                    case PositionType.utility:
                        SimulateLandOnPropertyRailOrUtility(_activePlayer, currentPosition);
                        break;
                    case PositionType.communitychest:
                        SimulateCommunityChest(_activePlayer, _players);
                        break;
                    case PositionType.tax:
                        SimulateLandOnTax(_activePlayer, currentPosition);
                        break;
                    case PositionType.chance:
                        SimulateLandOnChance(_activePlayer);
                        break;
                    case PositionType.gotojail:
                        SimulateLandOnGoToJail(_activePlayer);
                        break;
                    case PositionType.freeparking:
                    case PositionType.jail:
                    case PositionType.go:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SimulatePlayerRealEstateDecision(_activePlayer);

                if (true)
                {
                    Console.WriteLine("Id:{0}\tBalance:{1}\tRolls:{2}\tPositions:{3}\tAlive:{4}\tHouses{5}",
                        _activePlayer.Id,
                        _activePlayer.Balance,
                        _activePlayer.Rolls.Count,
                        _activePlayer.PositionsAcquired.Count,
                        _activePlayer.PlayerIsAlive,
                        _activePlayer.PositionsAcquired.Sum(pos => pos.BuildingCount));

                    if (++stepCount % _players.Count(player => player.PlayerIsAlive) == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine(stepCount);
                        Thread.Sleep(50);
                    }
                }

                _activePlayer = _players[GetNextPlayer(_activePlayer)];
            }

            Console.WriteLine();
            Console.WriteLine("Finished Game");
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

        private void SimulateLandOnGoToJail(Player activePlayer)
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
                .Where(p => p.group[0] != 9 && p.group[0] != 10)
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
                SimulateAuction(_players, property);
            activePlayer.KillPlayer();
        }

        private bool GameInProgress(IEnumerable<Player> players)
        {
            return players.Count(p => p.PlayerIsAlive) > 1;
        }

        private int GetNextPlayer(Player activePlayer)
        {
            var remainingPlayers = _players.Where(player => player.PlayerIsAlive || player.Id == activePlayer.Id)
                .ToList();
            return remainingPlayers[(remainingPlayers.IndexOf(activePlayer) + 1) % remainingPlayers.Count].Id;
        }

        private void SimulateLandOnPropertyRailOrUtility(Player activePlayer, Position currentPosition)
        {
            if (!activePlayer.OwnsPosition(currentPosition))
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
                        SimulateAuction(_players, currentPosition);
                }
            }
        }

        private void SimulateBankruptcyByPlayer(Player activePlayer, Player currentPositionOwner)
        {
            foreach (var property in activePlayer.PositionsAcquired)
            {
                property.DemolishBuildings();
                property.AssignNewOwner(currentPositionOwner);
                currentPositionOwner.InheritPropertyFromPlayer(property);
            }
            activePlayer.KillPlayer();
        }

        private void SimulateAuction(List<Player> players, Position currentPosition)
        {
            Player highestBidder = null;
            var highestBid = 20;
            const int minIncrease = 20;
            var higherBidderFound = true;
            while (higherBidderFound)
            {
                foreach (var bidder in players.Where(p => p.PlayerIsAlive))
                {
                    higherBidderFound = false;
                    if (bidder.WantsToBid(highestBid + minIncrease, currentPosition))
                    {
                        highestBidder = bidder;
                        highestBid += minIncrease;
                        higherBidderFound = true;
                    }
                }
            }
            highestBidder.PurchasePosition(currentPosition);
        }
    }
}