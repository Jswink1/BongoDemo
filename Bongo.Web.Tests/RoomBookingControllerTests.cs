using Bongo.Core.Services.IServices;
using Bongo.Models.Model;
using Bongo.Models.Model.VM;
using Bongo.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongo.Web
{
    [TestFixture]
    public class RoomBookingControllerTests
    {
        private Mock<IStudyRoomBookingService> _studyRoomBookingService;
        private RoomBookingController _bookingController;

        [SetUp]
        public void Setup()
        {
            _studyRoomBookingService = new Mock<IStudyRoomBookingService>();
            _bookingController = new RoomBookingController(_studyRoomBookingService.Object);
        }

        [Test]
        public void IndexPage_CallRequest_VerifyGetAllInvoked()
        {
            // Test that GetAllBooking() is called once 
            _bookingController.Index();
            _studyRoomBookingService.Verify(x => x.GetAllBooking(), Times.Once);
        }

        [Test]
        public void BookRoomPost_ModelStateInvalid_ReturnView()
        {
            // Add an error to the model state
            _bookingController.ModelState.AddModelError("test", "test");

            var result = _bookingController.Book(new StudyRoomBooking());

            // Test that if the modelstate is not valid the user is returned to the "Book" view
            ViewResult viewResult = result as ViewResult;
            Assert.AreEqual("Book", viewResult.ViewName);
        }

        [Test]
        public void BookRoomPost_NotSuccessful_ErrorNotification()
        {
            // Setup BookStudyRoom() to return a Code of NoRoomAvailable
            _studyRoomBookingService.Setup(x => x.BookStudyRoom(It.IsAny<StudyRoomBooking>()))
                .Returns(new StudyRoomBookingResult()
                {
                    Code = StudyRoomBookingCode.NoRoomAvailable
                });

            var result = _bookingController.Book(new StudyRoomBooking());

            // Test that the result is of type ViewResult
            Assert.IsInstanceOf<ViewResult>(result);

            // Test that the ViewData for "Error" is the expected message
            ViewResult viewResult = result as ViewResult;
            Assert.AreEqual("No Study Room available for selected date"
                , viewResult.ViewData["Error"]);

        }

        [Test]
        public void BookRoomPost_Success_NotificationAndRedirect()
        {
            // Setup BookStudyRoom() to return a BookingResult that is populated with properties of the request parameters
            _studyRoomBookingService.Setup(x => x.BookStudyRoom(It.IsAny<StudyRoomBooking>()))
                .Returns((StudyRoomBooking booking) => new StudyRoomBookingResult()
                {
                    Code = StudyRoomBookingCode.Success,
                    FirstName = booking.FirstName,
                    LastName = booking.LastName,
                    Date = booking.Date,
                    Email = booking.Email
                });
            
            var result = _bookingController.Book(new StudyRoomBooking()
            {
                Date = DateTime.Now,
                Email = "hello@dotnetmastery.com",
                FirstName = "Hello",
                LastName = "DotNetMastery",
                StudyRoomId = 1
            });

            // Test that the result is of type RedirectToActionResult
            Assert.IsInstanceOf<RedirectToActionResult>(result);

            // Test that the actionResult contains correct values for FirstName and ResultCode
            RedirectToActionResult actionResult = result as RedirectToActionResult;
            Assert.AreEqual("Hello", actionResult.RouteValues["FirstName"]);
            Assert.AreEqual(StudyRoomBookingCode.Success, actionResult.RouteValues["Code"]);
        }
    }
}
