using Microsoft.AspNetCore.Mvc;

namespace KidsLearningPlatform.Controllers
{
    public class LearningController : Controller
    {
        public IActionResult Alphabet()
        {
            return View();
        }
        public IActionResult Number()
        {
            return View();
        }
        public IActionResult Dictionary()
        {
            return View();
        }
    }

}
