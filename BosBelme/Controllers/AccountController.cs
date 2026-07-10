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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if ((!ModelState.IsValid) || (model.Password != model.ConfirmPassword)) return View(model);
            
            Users user;

            try
            {
                var task = _regService.RegistrationUserAsync(model.Name, model.Email, model.Password);
                user = await task;
            }
            catch (UserAlreadyExistsException ex)
            {
                return View(model);
            }
            catch (UserNameAlredyExistsException ex)
            {
                return View(model);
            }

            RegisterDto dto = RegisterDto.FromUser(user);
            await _cookieAuthService.SignInAsync(dto);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            Users user;
            try
            {
                var task = _authService.AuthenticationUserAsync(model.NameOrEmail, model.Password);
                user = await task;
            }
            catch (UserNotExistsException ex)
            {
                return View(model);
            }
            catch (UserPasswordWrongException ex)
            {
                return View(model);
            }

            RegisterDto dto = RegisterDto.FromUser(user);
            await _cookieAuthService.SignInAsync(dto);

            return RedirectToAction("Index", "Home");
        }
    }
}