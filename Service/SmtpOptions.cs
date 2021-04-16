using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Service
{
	public class SmtpOptions
	{
			public string Host { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public int Port { get; set; }
	}
}