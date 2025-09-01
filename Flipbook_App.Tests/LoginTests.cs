using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Flipbook_App.Pages;
using Flipbook_App.Repositories;
using Flipbook_App.Repositories.Interfaces;
using FlipBook_Library.Core;
using Flipbook_App.Data;

namespace Flipbook_App.Tests;

[TestFixture]
public class LoginTests
{
    private LoginModel CreateLoginModel(Mock<IUserRepository> userRepoMock = null)
    {
        userRepoMock ??= new Mock<IUserRepository>();
        var animRepoMock = new Mock<IAnimationRepository>();
        // Use in-memory db context for tests
        var options = new DbContextOptionsBuilder<FlipbookDBContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var dbContext = new FlipbookDBContext(options);
        var repoManager = new RepositoryManager(dbContext, userRepoMock.Object, animRepoMock.Object);
        var model = new LoginModel(repoManager)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(new DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };
        return model;
    }

    [Test]
    public async Task OnPostAsync_InvalidModelState_ReturnsPage()
    {
        var model = CreateLoginModel();
        model.ModelState.AddModelError("Test", "Error");
        var result = await model.OnPostAsync();
        Assert.That(result, Is.TypeOf<PageResult>());
    }

    [Test]
    public async Task OnPostAsync_UserNotFound_ReturnsPageWithError()
    {
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns((User)null);
        var model = CreateLoginModel(userRepoMock);
        model.Input = new LoginModel.LoginInput { Username = "user", Password = "pass" };
        var result = await model.OnPostAsync();
        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(model.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task OnPostAsync_InvalidPasswordHashFormat_ReturnsPageWithError()
    {
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns(new User { Username = "user", PasswordHash = "badformat" });
        var model = CreateLoginModel(userRepoMock);
        model.Input = new LoginModel.LoginInput { Username = "user", Password = "pass" };
        var result = await model.OnPostAsync();
        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(model.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task OnPostAsync_WrongPassword_ReturnsPageWithError()
    {
        var salt = new byte[16];
        new Random().NextBytes(salt);
        var hash = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var passwordHash = $"{Convert.ToBase64String(salt)}:{hash}";
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns(new User { Username = "user", PasswordHash = passwordHash });
        var model = CreateLoginModel(userRepoMock);
        model.Input = new LoginModel.LoginInput { Username = "user", Password = "wrongpass" };
        var result = await model.OnPostAsync();
        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(model.ModelState.ErrorCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task OnPostAsync_ValidLogin_RedirectsToProjects()
    {
        var salt = new byte[16];
        new Random().NextBytes(salt);
        var password = "correctpass";
        var hash = Convert.ToBase64String(Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256,
            iterationCount: 100_000,
            numBytesRequested: 32));
        var passwordHash = $"{Convert.ToBase64String(salt)}:{hash}";
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(r => r.GetByUsername(It.IsAny<string>())).Returns(new User { Username = "user", PasswordHash = passwordHash });
        var model = CreateLoginModel(userRepoMock);
        model.Input = new LoginModel.LoginInput { Username = "user", Password = password };
        // Setup authentication
        var authServiceMock = new Mock<IAuthenticationService>();
        model.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(authServiceMock.Object)
            .BuildServiceProvider();
        var result = await model.OnPostAsync();
        var redirect = result as RedirectToPageResult;
        Assert.That(redirect, Is.Not.Null);
        Assert.That(redirect.PageName, Is.EqualTo("/Projects"));
    }
}
