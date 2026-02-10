using Microsoft.AspNetCore.Mvc;

namespace RFIDP2P3_API.Controllers
{
	public class InteraceRFIDReaderController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
