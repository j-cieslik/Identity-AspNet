using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data
{
	public class ApplicationDbContext : IdentityDbContext
	{

		protected ApplicationDbContext()
		{}
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{}

		
	}
}