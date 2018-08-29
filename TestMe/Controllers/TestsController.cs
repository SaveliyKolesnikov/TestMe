﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestMe.Data;
using TestMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TestMe.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private string _userId;
        public TestsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _userId = _userManager.GetUserId(User);
        }
        // GET: Tests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tests.Include(t => t.AppUser).Where(t => t.AppUserId == _userId);

            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<IActionResult> UserResults(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.AppUser)
                .Include(t=> t.TestResults)
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(test.TestResults);
        }
        public async Task<IActionResult> StopSharing(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                test.TestCode = null;
                _context.Update(test);
                _context.Entry<Test>(test).Property(x => x.TestDuration).IsModified = false;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!TestExists(test.Id))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> CreateCode(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }
            if (test.TestCode is null)
            {
                var generatedCode = RandomString(8);
                try
                {
                    while (_context.Tests.Any(t => t.TestCode == generatedCode))
                        generatedCode = RandomString(8);

                    test.TestCode = generatedCode;
                    _context.Update(test);
                    _context.Entry<Test>(test).Property(x => x.CreationDate).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (!TestExists(test.Id))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                return View("CreateCode", generatedCode);
            }
            return View("CreateCode", test.TestCode);
        }
        private string RandomString(int length)
        {
             Random random = new Random();
             const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
             return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.AppUser)
                .Include(t=> t.TestQuestions)
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Test test)
        {
            if (_context.Tests.Where(t => t.AppUserId == _userId).Any(t => t.TestName == test.TestName))
            {
                ModelState.AddModelError("TestName", "You already have test with the same name!");
            }
            if (ModelState.IsValid)
            {
                test.AppUserId = _userId;
                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == id && t.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TestName,CreationDate, TestDuration")] Test test)
        {
            if (id != test.Id)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    test.AppUserId = _userId;
                    _context.Update(test);
                    _context.Entry<Test>(test).Property(x => x.CreationDate).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.Id))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(test);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == _userId);
            if (test == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }
    }
}
