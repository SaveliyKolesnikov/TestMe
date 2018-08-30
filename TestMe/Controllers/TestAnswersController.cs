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

namespace TestMe.Controllers
{
    [Authorize]
    public class TestAnswersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private string _userId;
        public TestAnswersController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Get user id   
            _userId = _userManager.GetUserId(User);
        }
        // GET: TestAnswers
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testQuestion = await _context.TestQuestions.FindAsync(id);
            if (testQuestion == null)
            {
                return NotFound();
            }
            ViewBag.TestId = testQuestion.TestId;
            ViewBag.TestQuestionId = testQuestion.Id;
            ViewBag.TestQuestionText = testQuestion.QuestionText;
            var applicationDbContext = _context.TestAnswers.Include(t => t.AppUser).Include(t => t.TestQuestion).Where(ta => ta.TestQuestionId == id);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TestAnswers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAnswer = await _context.TestAnswers
                .Include(t => t.AppUser)
                .Include(t => t.TestQuestion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testAnswer == null)
            {
                return NotFound();
            }

            return View(testAnswer);
        }

        // GET: TestAnswers/Create
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testQuestion = await _context.TestQuestions.FindAsync(id);
            if (testQuestion == null)
            {
                return NotFound();
            }
            ViewBag.TestQuestionId = testQuestion.Id;
            ViewBag.TestQuestionText = testQuestion.QuestionText;
            return View();
        }

        // POST: TestAnswers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnswerText,IsCorrect,TestQuestionId")] TestAnswer testAnswer)
        {
            if (ModelState.IsValid)
            {
                testAnswer.AppUserId = _userId;
                _context.Add(testAnswer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = testAnswer.TestQuestionId });
            }
            return View(testAnswer);
        }

        // GET: TestAnswers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAnswer = await _context.TestAnswers.Include(t => t.AppUser)
                .Include(t => t.TestQuestion).FirstOrDefaultAsync(t => t.Id == id);
            if (testAnswer == null)
            {
                return NotFound();
            }
            ViewBag.TestQuestionId = testAnswer.TestQuestionId;
            ViewBag.TestQuestionText = testAnswer.TestQuestion.QuestionText;
            return View(testAnswer);
        }

        // POST: TestAnswers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, AnswerText,IsCorrect,TestQuestionId")] TestAnswer testAnswer)
        {
            if (id != testAnswer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    testAnswer.AppUserId = _userId;
                    _context.Update(testAnswer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestAnswerExists(testAnswer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = testAnswer.TestQuestionId });
            }
            return View(testAnswer);
        }

        // GET: TestAnswers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testAnswer = await _context.TestAnswers
                .Include(t => t.AppUser)
                .Include(t => t.TestQuestion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testAnswer == null)
            {
                return NotFound();
            }

            return View(testAnswer);
        }

        // POST: TestAnswers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testAnswer = await _context.TestAnswers.FindAsync(id);
            _context.TestAnswers.Remove(testAnswer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = testAnswer.TestQuestionId });
        }

        private bool TestAnswerExists(int id)
        {
            return _context.TestAnswers.Any(e => e.Id == id);
        }
    }
}
