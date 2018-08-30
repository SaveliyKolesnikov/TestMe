﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using TestMe.Sevices.Interfaces;

namespace TestMe.Controllers
{
    [Authorize]
    public class TestQuestionsController : Controller
    {
        private readonly ITestingPlatform _testingPlatform;
        private readonly UserManager<AppUser> _userManager;
        private string _userId;
        public TestQuestionsController(ITestingPlatform testingPlatform, UserManager<AppUser> userManager)
        {
            _testingPlatform = testingPlatform;
            _userManager = userManager;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _userId = _userManager.GetUserId(User);
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            var test = await _testingPlatform.TestManager.GetTestAsync(_userId, id);
            if (test == null)
            {
                return RedirectToAction("Index", "Tests");
            }
            ViewBag.TestId = test.Id;
            ViewBag.TestName = test.TestName;
            var questions = _testingPlatform.TestQuestionManager.GetAll();
            var applicationDbContext = _testingPlatform.TestQuestionManager.GetAll().Where(t => t.AppUser.Id == _userId && t.TestId == id);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> DetailsAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            var testQuestion = await _testingPlatform.TestQuestionManager.GetTestQuestionAsync(_userId, id);
            if (testQuestion == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            return View(testQuestion);
        }

        public async Task<IActionResult> CreateAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            var test = await _testingPlatform.TestManager.GetTestAsync(_userId, id);
            if (test == null)
            {
                return RedirectToAction("Index", "Tests");
            }
            ViewBag.TestId = test.Id;
            ViewBag.TestName = test.TestName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionText", "TestId")] TestQuestion testQuestion)
        {
            if (ModelState.IsValid)
            {
                testQuestion.AppUserId = _userId;
                await _testingPlatform.TestQuestionManager.AddAsync(testQuestion);
                return RedirectToAction(nameof(Index), new { id = testQuestion.TestId });
            }
            return View();
        }

        public async Task<IActionResult> EditAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            var testQuestion = await _testingPlatform.TestQuestionManager.GetTestQuestionAsync(_userId, id);
            if (testQuestion == null)
            {
                return RedirectToAction("Index", "Tests");
            }
            ViewBag.TestId = testQuestion.TestId;
            return View(testQuestion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,QuestionText,TestId")] TestQuestion testQuestion)
        {
            if (id != testQuestion.Id)
            {
                return RedirectToAction("Index", "Tests");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testQuestion.AppUserId = _userId;
                    await _testingPlatform.TestQuestionManager.UpdateAsync(testQuestion);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_testingPlatform.TestQuestionManager.GetTestQuestionAsync(_userId, id) is null)
                    {
                        return RedirectToAction("Index", "Tests");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = testQuestion.TestId });
            }
            return View(testQuestion);
        }

        public async Task<IActionResult> DeleteAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            var testQuestion = await _testingPlatform.TestQuestionManager.GetTestQuestionAsync(_userId, id);
            if (testQuestion == null)
            {
                return RedirectToAction("Index", "Tests");
            }

            return View(testQuestion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testQuestion = await _testingPlatform.TestQuestionManager.GetTestQuestionAsync(_userId, id);
            var testId = testQuestion.TestId;
            await _testingPlatform.TestQuestionManager.DeleteAsync(testQuestion);
            return RedirectToAction(nameof(Index), new { id = testId });
        }

    }
}
