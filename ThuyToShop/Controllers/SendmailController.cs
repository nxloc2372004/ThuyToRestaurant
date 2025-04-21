using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using MimeKit;

namespace ThuyTo.Controllers
{
	public class SendmailController : Controller
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		public SendmailController(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}
		public IActionResult Index()
		{
			using(var client = new SmtpClient())
			{
                client.Connect("smtp.gmail.com", 465);
                client.Authenticate("tongthuan15092003@gmail.com", "wcnifnnsjnyjkvlr");
                var bodyBuiler = new BodyBuilder();

				var path = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "EmailTemplate" + Path.DirectorySeparatorChar.ToString() + "ConfirmOrder.html";
				string HtmlBody = "";
				using (StreamReader streamReader = System.IO.File.OpenText(path))
				{
					HtmlBody = streamReader.ReadToEnd();
				}
				string messageBody = string.Format(HtmlBody, "Bùi Thị Thủy");
				bodyBuiler.HtmlBody = messageBody;
				var message = new MimeMessage
				{
					Body = bodyBuiler.ToMessageBody()
				};

				message.From.Add(new MailboxAddress("Thủy Tồ Shops", "tongthuan15092003@gmail.com"));
				message.To.Add(new MailboxAddress("test", "trithanh7650@gmail.com"));
				message.Subject = "test mail";
				client.Send(message);
				client.Disconnect(true);
			}
			return View();
		}
	}
}
