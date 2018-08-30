﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestMe.Data.Interfaces;
using TestMe.Models;

namespace TestMe.Sevices.Interfaces
{
    public interface ITestAnswerManager : IRepository<TestAnswer>
    {
        TestAnswer GetTestAnswer(string userId, int? id);
    }
}
