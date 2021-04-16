using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Service;

namespace Identity
{
	public class Startup
	{
		private readonly IConfiguration _config;
		public Startup(IConfiguration config)
		{
			_config = config;

		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
					services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_config.GetConnectionString("DefaultConnection")));
					services.AddIdentity<IdentityUser, IdentityRole>()
												.AddEntityFrameworkStores<ApplicationDbContext>()
												.AddDefaultTokenProviders();
												
					services.Configure<IdentityOptions>(options => {

						options.Password.RequiredLength = 3;
						options.Password.RequireDigit = true;
						options.Password.RequireNonAlphanumeric = false;

						options.Lockout.MaxFailedAccessAttempts = 3;
						options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

						options.SignIn.RequireConfirmedEmail = true;

					});

					services.ConfigureApplicationCookie(option => {

						option.LoginPath = "/Identity/Signin";
						option.AccessDeniedPath = "/Identity/AccessDenied";
						option.ExpireTimeSpan = TimeSpan.FromHours(10);

					});

					services.Configure<SmtpOptions>(_config.GetSection("Smtp"));
					services.AddControllersWithViews();

					services.AddAuthorization(option => {

							option.AddPolicy("MemberDep", p => {
								p.RequireClaim("Department", "it").RequireRole("Member");
							});

							option.AddPolicy("AdminDep", p => {
								p.RequireClaim("Department", "Tech").RequireRole("Admin");
							});
					});

					services.AddSingleton<IEmailSender, SmtpEmailSender>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
									name: "default",
									pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
