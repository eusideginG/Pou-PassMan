using Isopoh.Cryptography.Argon2;
using System;

namespace Pou_Pass_Man.Helper
{
    /// <summary>
    /// A class for creating Argon2 configuration
    /// </summary>
    internal static class Argon2Helper
    {
        public static Argon2Config config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            Version = Argon2Version.Nineteen,
            TimeCost = 10,
            MemoryCost = 32768,
            Lanes = 5,
            Threads = Environment.ProcessorCount,
            Salt = new byte[32],
            HashLength = 20
        };
    }
}
