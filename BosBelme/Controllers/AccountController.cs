using BosBelme.ViewModels;
using BosBelme.Service.Dto;
using BosBelme.Service.Service;
using BosBelme.Service.IService;
using BosBelme.Service.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BosBelme.Data;
using BosBelme.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using BosBelme.Service.Extension;

namespace BosBelme.Controllers
{
    public class AccountController : Controller
    {
        // объекты сервисов
        private readonly IAuthService _authService;
        private readonly IRegService _regService;
        private readonly ICookieAuthService _cookieAuthService;
        
        public AccountController(
            IAuthService authService,
            IRegService regService,
            ICookieAuthService cookieAuthService)
        {
            _authService = authService;
            _regService = regService;
            _cookieAuthService = cookieAuthService;
        }

        public IActionResult Register() => View();
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if ((!ModelState.IsValid) || (model.Password != model.ConfirmPassword)) return View(model);

            try
            {
                // Регистрация нового пользователя
                var user = await _regService.RegistrationUserAsync(model.Name, model.Email, model.Password);

                await _cookieAuthService.SignInAsync(user.FromUser());

                return RedirectToAction("Index", "Home");
            }
            catch (UserAlreadyExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch( UserNameAlreadyExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            } 
            catch( Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                //Авторизация нового пользователя
                var user = await _authService.AuthenticationUserAsync(model.NameOrEmail, model.Password);

                await _cookieAuthService.SignInAsync(user.FromUser());

                return RedirectToAction("Index", "Home");
            }
            catch (UserNotExistsException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (UserPasswordWrongException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}