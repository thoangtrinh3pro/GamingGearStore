using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminFeedbackController : Controller
{
    private readonly IRepository<Feedback> _feedbackRepo;

    public AdminFeedbackController(IRepository<Feedback> feedbackRepo)
    {
        _feedbackRepo = feedbackRepo;
    }

    // GET: /AdminFeedback
    public async Task<IActionResult> Index()
    {
        var feedbacks = await _feedbackRepo.GetAllAsync();
        var sorted = feedbacks.OrderByDescending(f => f.CreatedAt).ToList();
        return View(sorted);
    }
}