using System;

namespace MonopolySimulator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new GameState(new Random(), 4, 1200).RunSimulation();
            Console.ReadLine();
        }
    }
}

//internal enum PositionNameUK
//{
//    Go = 0,
//    Old_Kent_Road,
//    Community_Chest_1,
//    Whitechapel_Road,
//    Income_Tax,
//    Kings_Cross_Station,
//    The_Angel_Islington,
//    Chance_1,
//    Euston_Road,
//    Pentonville_Road,
//    Jail,
//    Pall_Mall,
//    Electric_Company,
//    Whitehall,
//    Northumberland_Avenue,
//    Marylebone_Station,
//    Bow_Street,
//    Community_Chest_2,
//    Marlborough_Street,
//    Vine_Street,
//    Free_Parking,
//    Strand,
//    Chance_2,
//    Fleet_Street,
//    Trafalgar_Square,
//    Fenchurch_Street_Station,
//    Leicester_Square,
//    Coventry_Street,
//    Water_Works,
//    Piccadilly,
//    Go_To_Jail,
//    Regent_Street,
//    Oxford_Street,
//    Community_Chest_3,
//    Bond_Street,
//    Liverpool_Street_Station,
//    Chance_3,
//    Park_Lane,
//    Super_Tax,
//    Mayfair
//}