using Bongo.Core.Services;
using Bongo.DataAccess.Repository.IRepository;
using Bongo.Models.Model;
using Bongo.Models.Model.VM;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongo.Core
{
    [TestFixture]
    public class StudyRoomBookingServiceTests
    {
        private Mock<IStudyRoomBookingRepository> _studyRoomBookingRepoMock;
        private Mock<IStudyRoomRepository> _studyRoomRepoMock;
        private StudyRoomBookingService _bookingService;
        private StudyRoomBooking _request;
        private List<StudyRoom> _availableStudyRoom;

        [SetUp]
        public void Setup()
        {
            // Populate booking request with test values
            _request = new StudyRoomBooking
            {
                FirstName = "Ben",
                LastName = "Spark",
                Email = "ben@gmail.com",
                Date = new DateTime(2022, 1, 1)
            };

            // Populate list of available rooms with test values
            _availableStudyRoom = new List<StudyRoom> {
                new StudyRoom{
                    Id=10,RoomName="Michigan", RoomNumber="A202"
                }
            };

            // Initialize mock dependencies
            _studyRoomBookingRepoMock = new Mock<IStudyRoomBookingRepository>();
            _studyRoomRepoMock = new Mock<IStudyRoomRepository>();
            // Setup GetAll() to return the test list of available rooms
            _studyRoomRepoMock.Setup(x => x.GetAll()).Returns(_availableStudyRoom);

            // Initialize the booking service with the mock dependencies
            _bookingService = new StudyRoomBookingService(
                _studyRoomBookingRepoMock.Object,
                _studyRoomRepoMock.Object
                );            
        }
        
        [TestCase]
        public void GetAllBooking_InvokeMethod_CheckIfRepoIsCalled()
        {
            // Test that GetAll() is called Once when the service calls GetAllBooking()
            _bookingService.GetAllBooking();
            _studyRoomBookingRepoMock.Verify(x => x.GetAll(null), Times.Once);
        }
        
        [TestCase]
        public void BookingException_NullRequest_ThrowsException()
        {
            // Test that the BookStudyRoom() throws a ArgumentNullException if the request is null
            var exception = Assert.Throws<ArgumentNullException>(
                () => _bookingService.BookStudyRoom(null));

            // Test that the exception message is equal to what is expected
            Assert.AreEqual("Value cannot be null. (Parameter 'request')", exception.Message);

            // Test that the name of the parameter in the exception is "request"
            Assert.AreEqual("request", exception.ParamName);
        }

        // Test that BookStudyRoom() is successfull and returns the result
        [Test]
        public void BookStudyRoom_SaveBookingWithAvailableRoom_ReturnsResultWithAllValues()
        {
            // The booking the user intends to save to the DB
            StudyRoomBooking savedStudyRoomBooking = null;

            // Setup Book() to take any StudyRoomBooking
            // and then assign the StudyRoomBooking to savedStudyRoomBooking
            _studyRoomBookingRepoMock.Setup(x => x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(booking =>
                {
                    savedStudyRoomBooking = booking;
                });
            
            _bookingService.BookStudyRoom(_request);

            // Test that Book() is called once
            _studyRoomBookingRepoMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Once);
            // Test that the study room that was saved is not null
            Assert.NotNull(savedStudyRoomBooking);
            // Test that the request properties match the savedStudyRoomBooking
            Assert.AreEqual(_request.FirstName, savedStudyRoomBooking.FirstName);
            Assert.AreEqual(_request.LastName, savedStudyRoomBooking.LastName);
            Assert.AreEqual(_request.Email, savedStudyRoomBooking.Email);
            Assert.AreEqual(_request.Date, savedStudyRoomBooking.Date);
            Assert.AreEqual(_availableStudyRoom.First().Id, savedStudyRoomBooking.StudyRoomId);
        }

        // Test that BookStudyRoom() request parameter matches the result
        [Test]
        public void BookStudyRoom_InputRequest_MatchesResult()
        {
            StudyRoomBookingResult result = _bookingService.BookStudyRoom(_request);

            Assert.NotNull(result);
            Assert.AreEqual(_request.FirstName, result.FirstName);
            Assert.AreEqual(_request.LastName, result.LastName);
            Assert.AreEqual(_request.Email, result.Email);
            Assert.AreEqual(_request.Date, result.Date);
        }

        // Test that BookStudyRoom() returns a correct result.code when rooms are available and unavailable
        [TestCase(true, ExpectedResult = StudyRoomBookingCode.Success)]
        [TestCase(false, ExpectedResult = StudyRoomBookingCode.NoRoomAvailable)]
        public StudyRoomBookingCode BookStudyRoom_RoomAvability_ReturnsCorrectResultCode(bool roomAvailability)
        {
            if (roomAvailability == false)
            {
                // Remove available rooms from the list
                _availableStudyRoom.Clear();
            }
            return _bookingService.BookStudyRoom(_request).Code;
        }
        
        [TestCase(0, false)]
        [TestCase(55, true)]
        public void BookStudyRoom_BookAvailableRoom_ReturnsBookingId(int expectedBookingId, bool roomAvailability)
        {
            if (roomAvailability == false)
            {
                // Remove available rooms from the list
                _availableStudyRoom.Clear();
            }

            // Setup Book() to take any StudyRoomBooking
            // and then assign the BookingId to 55
            _studyRoomBookingRepoMock.Setup(x => x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(booking =>
                {
                    booking.BookingId = 55;
                });

            // Test that the BookingId is populated if an available room is booked,
            // or that the BookingId is 0 if there are no available rooms
            var result = _bookingService.BookStudyRoom(_request);
            Assert.AreEqual(expectedBookingId, result.BookingId);
        }

        [Test]
        public void BookNotInvoked_SaveBookingWithoutAvailableRoom_BookMethodNotInvoked()
        {
            _availableStudyRoom.Clear();
            var result = _bookingService.BookStudyRoom(_request);

            // Test that Book() is never called if there are no available rooms
            _studyRoomBookingRepoMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Never);

        }
    }
}
