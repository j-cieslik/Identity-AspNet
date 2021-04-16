using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Identity.Models;
using Service;
using System.Security.Claims;

namespace Controllers
{
	public class IdentityController : Controller
	{

		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmailSender _emailSender;
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public IdentityController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
		{
			_roleManager = roleManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_userManager = userManager;
		}

		public async Task<IActionResult> Signup()
		{
			var model = new SignupViewModel() {Role = "Member"};
			return View(model);
		}

		[HttpPost]
		public IActionResult ExternalLogin(string provider, string returnUrl=null)
		{
				var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
				var callBackUrl = Url.Action("ExternalLoginCallback");
				properties.RedirectUri = callBackUrl;
				
				return Challenge(properties, provider);
		}

		public async Task<IActionResult> ExternalLoginCallback()
		{
			var info = await _signInManager.GetExternalLoginInfoAsync();
			var emailClaim = info.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
			var user = new IdentityUser {Email = emailClaim.Value, UserName = emailClaim.Value};

			if ((await _userManager.FindByEmailAsync(user.Email)) == null)
			{
					await _userManager.CreateAsync(user);
					await _userManager.AddLoginAsync(user, info);
			}

			await _signInManager.SignInAsync(user, false);

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> Signup(SignupViewModel model)
		{
			if (ModelState.IsValid)
			{

				if (!(await _roleManager.RoleExistsAsync(model.Role)))
				{
					var role = new IdentityRole {Name = model.Role};
					var roleResult = await _roleManager.CreateAsync(role);

					if (!roleResult.Succeeded)
					{
						var errors = roleResult.Errors.Select(s => s.Description);
						ModelState.AddModelError("Role", string.Join(",", errors));
						return View(model);
					}
				}

				if ((await _userManager.FindByEmailAsync(model.Email)) == null)
				{

					var user = new IdentityUser
					{
						Email = model.Email,
						UserName = model.Email,
					};

					var result = await _userManager.CreateAsync(user, model.Password);
					user = await _userManager.FindByEmailAsync(model.Email);

					var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

					if (result.Succeeded)
					{
						var claim = new Claim("Department", model.Department);
						await _userManager.AddClaimAsync(user, claim);

						await _userManager.AddToRoleAsync(user, model.Role);

						var confirmationLink = Url.ActionLink("ConfirmEmail", "Identity", new { userId = user.Id, @token = token });
						await _emailSender.SendEmailAsync("cieslA3@op.pl", user.Email, "Confirm your email address", confirmationLink);

						return RedirectToAction("Signin");
					}

					ModelState.AddModelError("Signup", string.Join("", result.Errors.Select(x => x.Description)));
					return View(model);
				}
			}

			return View(model);
		}

		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);

			var result = await _userManager.ConfirmEmailAsync(user, token);

			if (result.Succeeded)
			{
				return RedirectToAction("Signin");
			}

			return new NotFoundResult();
		}


		public IActionResult Signin()
		{
			return View(new SigninViewModel());
		}

		[HttpPost]
		public async Task<IActionResult> Signin(SigninViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

				if (result.Succeeded)
				{
					var user = await _userManager.FindByEmailAsync(model.Username);

					var userClaims = await _userManager.GetClaimsAsync(user);

					if (await _userManager.IsInRoleAsync(user, "Member"))
					{
							return RedirectToAction("Member", "Home");
					}

				}
				else
				{
					ModelState.AddModelError("Login", "Cannot login");
				}
			}

			return View(model);
		}

		public async Task<IActionResult> AccessDenied()
		{
			return View();
		}

		public async Task<IActionResult> Signout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Signin");
		}

	}
}