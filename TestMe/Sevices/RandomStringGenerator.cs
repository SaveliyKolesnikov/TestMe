﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMe.Sevices.Interfaces;

namespace TestMe.Sevices
{
    public class RandomStringGenerator : IRandomStringGenerator
    {
        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
