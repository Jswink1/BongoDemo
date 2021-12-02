using Bongo.DataAccess.Repository;
using Bongo.Models.Model;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// These tests for the DataAccess are probably the least necessary tests for this app.
// We are not doing anything complex with EFCore, and EFCore usually runs well without many problems.
// Adding an In-Memory database and these tests is adding a bit of bulk to our application.

namespace Bongo.DataAccess
{
    [TestFixture]
    public class StudyRoomBookingRepositoryTests
    {
        private StudyRoomBooking studyRoomBooking_One;
        private StudyRoomBooking studyRoomBooking_Two;
        private DbContextOptions<ApplicationDbContext> options;

        public StudyRoomBookingRepositoryTests()
        {
            studyRoomBooking_One = new StudyRoomBooking()
            {
                FirstName = "Ben1",
                LastName = "Spark1",
                Date = new DateTime(2023, 1, 1),
                Email = "ben1@gmail.coom",
                BookingId = 11,
                StudyRoomId = 1
            };

            studyRoomBooking_Two = new StudyRoomBooking
            {
                FirstName = "Ben2",
                LastName = "Spark2",
                Date = new DateTime(2023, 2, 2),
                Email = "ben2@gmail.coom",
                BookingId = 22,
                StudyRoomId = 2
            };
        }

        [SetUp]
        public void Setup()
        {
            // Create In-Memory DB
            options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase("temp_Bongo").Options;
        }

        [Test]
        [Order(1)]
        public void SaveBooking_Booking_One_CheckTheValuesFromDatabase()
        {
            // Open connection and create booking for studyRoomBooking_One
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new StudyRoomBookingRepository(context);
                repository.Book(studyRoomBooking_One);
            }

            // Read and test the values from the DB
            using (var context = new ApplicationDbContext(options))
            {
                var bookingFromDb = context.StudyRoomBookings.FirstOrDefault(u => u.BookingId == 11);
                Assert.AreEqual(studyRoomBooking_One.BookingId, bookingFromDb.BookingId);
                Assert.AreEqual(studyRoomBooking_One.FirstName, bookingFromDb.FirstName);
                Assert.AreEqual(studyRoomBooking_One.LastName, bookingFromDb.LastName);
                Assert.AreEqual(studyRoomBooking_One.Email, bookingFromDb.Email);
                Assert.AreEqual(studyRoomBooking_One.Date, bookingFromDb.Date);
            }
        }

        [Test]
        [Order(2)]
        public void GetAllBookings_Booking_OneAndTwo_CheckBothValuesFromDatabase()
        {            
            using (var context = new ApplicationDbContext(options))
            {
                // Delete the previous entires in the DB
                context.Database.EnsureDeleted();

                var repository = new StudyRoomBookingRepository(context);
                repository.Book(studyRoomBooking_One);
                repository.Book(studyRoomBooking_Two);
            }

            List<StudyRoomBooking> actualResult;
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new StudyRoomBookingRepository(context);
                actualResult = repository.GetAll(null).ToList();
            }

            var expectedResults = new List<StudyRoomBooking> { studyRoomBooking_One, studyRoomBooking_Two };

            CollectionAssert.AreEqual(expectedResults, actualResult, new BookingCompare());
        }

        // Class to compare two booking entries. IComparer is an interface used to compare two objects
        private class BookingCompare : IComparer
        {
            public int Compare(object x, object y)
            {
                var booking1 = (StudyRoomBooking)x;
                var booking2 = (StudyRoomBooking)y;

                // If the booking entries are similar return 0.
                // If they are different return 1
                if (booking1.BookingId != booking2.BookingId)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
