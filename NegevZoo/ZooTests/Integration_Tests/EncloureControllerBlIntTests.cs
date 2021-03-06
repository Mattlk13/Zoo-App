using NegevZoo.Controllers;
using DAL;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Web.Http;
using BL;
using System;
using DAL.Models;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace ZooTests
{
    [TestClass]
    public class EncloureControllerBlIntegrationTests
    {
        #region tests SetUp and TearDown

        private EnclosureController enclosureController;
        private int nonExistantLang;

        [TestInitialize]
        public void SetUp()
        {
            // The line below must be in every setup of each test. otherwise it will not be in a testing environment.
            ControllerBase.isTesting = true;
            enclosureController = new EnclosureController();
            enclosureController.Request = new HttpRequestMessage();
            enclosureController.Request.Headers.Add("Cookie","session-id=123");
            enclosureController.Request.RequestUri = new Uri("http://localhost:50000");
            nonExistantLang = 100;
        }

        [TestCleanup]
        public void EnclosureCleanUp()
        {
            DummyDB.CleanDb();
        }

        #endregion

        #region GetAllEnclosureResults()
        [TestMethod]
        public void GetAllEnclosuresResutlsLangHe()
        {
            Assert.AreEqual(4, enclosureController.GetAllEnclosureResults((int)Languages.he).Count());
        }

        [TestMethod]
        public void GetAllEnclosuresResultsLangEng()
        {
            Assert.AreEqual(4, enclosureController.GetAllEnclosureResults((int)Languages.en).Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetAllEnclosuresResultsLangNotExist()
        {
            enclosureController.GetAllEnclosureResults(nonExistantLang);
        }
        #endregion
        
        #region GetAllEnclosures()
        [TestMethod]
        public void GetAllEnclosures()
        {
            IEnumerable<Enclosure> encs = enclosureController.GetAllEnclosures();

            Assert.AreEqual(4, encs.Count());
        }
        #endregion

        #region GetEnclosureDetailsById
        [TestMethod]
        public void GetEnclosureDetailsByIdValidInput()
        {
            // 2 details
            IEnumerable<EnclosureDetail> details = enclosureController.GetEnclosureDetailsById(1);

            Assert.AreEqual(2, details.Count());

            EnclosureDetail enc = details.First();
            Assert.AreEqual("תצוגת הקופים", enc.name);

            enc = details.Last();
            Assert.AreEqual("Monkeys", enc.name);

            //1 detial
            details = enclosureController.GetEnclosureDetailsById(4);
            Assert.AreEqual(1, details.Count());

            enc = details.First();
            Assert.AreEqual("קרנף לבן", enc.name);
        }

        [TestMethod]
        public void GetEnclosureDetailsByIdWrongId()
        {
            var details = enclosureController.GetEnclosureDetailsById(-4);
            Assert.AreEqual(0, details.Count());
        }
        #endregion

        #region GetAllRecurringEvents

        [TestMethod]
        public void GetAllRecutnigEventsValidInput()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.he);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = encs.ElementAt(0);
            Assert.IsNotNull(enc);
            Assert.AreEqual(1, enc.Id);
            Assert.AreEqual("תצוגת הקופים", enc.Name);

            var recurringEvents = enclosureController.GetRecurringEvents((int)enc.Id, (int)Languages.he);
            Assert.IsNotNull(recurringEvents);
            Assert.AreEqual(1, recurringEvents.Count());

            var recEvent = recurringEvents.ElementAt(0);
            Assert.IsNotNull(recEvent);
            Assert.AreEqual(1, recEvent.id);
            Assert.AreEqual(enc.Id, recEvent.enclosureId);
            Assert.AreEqual(enc.Language, recEvent.language);
            Assert.AreEqual(new TimeSpan(10, 30, 0), recEvent.startTime);
            Assert.AreEqual(new TimeSpan(11, 0, 0), recEvent.endTime);
        }

        [TestMethod]
        public void GetRecurringEventValidInputEmptyList()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.he);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = encs.SingleOrDefault(en => en.Name == "זברה");
            Assert.IsNotNull(enc);
            Assert.AreEqual(3, enc.Id);

            var recurringEvents = enclosureController.GetRecurringEvents((int)enc.Id, (int)Languages.he);
            Assert.IsNotNull(recurringEvents);
            Assert.AreEqual(0, recurringEvents.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetRecurringEventsWrongId()
        {
            var recEvents = enclosureController.GetRecurringEvents(200);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetRecurringEventsWrongLanguage()
        {
            enclosureController.GetRecurringEvents(nonExistantLang);
        }
        #endregion

        #region GetEnclosurePicturesById
        [TestMethod]
        public void GetEnclosurePictureByIdValidInput()
        {
            //only 1 result
            var pictures = enclosureController.GetEnclosurePicturesById(1);
            Assert.AreEqual(1, pictures.Count());
            Assert.AreEqual("url1", pictures.First().pictureUrl);

            //more than 1
            pictures = enclosureController.GetEnclosurePicturesById(2);
            Assert.AreEqual(2, pictures.Count());
            Assert.AreEqual("url2", pictures.First().pictureUrl);
            Assert.AreEqual("url3", pictures.Last().pictureUrl);
        }

        [TestMethod]
        public void GetEnclosurePictureByIdEmptyUrl()
        {
            //only 1 result
            var pictures = enclosureController.GetEnclosurePicturesById(4);
            Assert.AreEqual(0, pictures.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetEnclosurePictureByIdWrongId()
        {
            //only 1 result
            var pictures = enclosureController.GetEnclosurePicturesById(-4);
        }
        #endregion

        #region GetEnclosureVideosUrlById
        [TestMethod]
        public void GetEnclosureVideoUrlByIdValidInput()
        {
            //only 1 result
            var videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(1, videos.Count());
            Assert.AreEqual("video1", videos.First().videoUrl);

            //more than 1
            videos = enclosureController.GetEnclosureVideosById(2);
            Assert.AreEqual(2, videos.Count());
            Assert.AreEqual("video2", videos.First().videoUrl);
            Assert.AreEqual("video3", videos.Last().videoUrl);
        }

        [TestMethod]
        public void GetEnclosureVideoByIdEmptyUrl()
        {
            //only 1 result
            var videos = enclosureController.GetEnclosureVideosById(4);
            Assert.AreEqual(0, videos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetEnclosureVideoByIdWrongId()
        {
            enclosureController.GetEnclosureVideosById(-4);
        }
        #endregion

        #region UpdateEnclosure
        [TestMethod]
        public void UpdateEnclosureAddEncValidTest()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.AreEqual(4, encs.Count());

            var enc = new Enclosure
            {
                id = default(int),
                name = "Lions enclosure"
            };

            enclosureController.UpdateEnclosure(enc);

            encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.AreEqual(4, encs.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureAddExistingName()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new Enclosure
            {
                id = default(int),
                name = "תצוגת הקופים",
                //story = "Finalt they are here",
                //language = (int)Languages.en
            };

            enclosureController.UpdateEnclosure(enc);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureAddEmptyName()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new Enclosure
            {
                id = default(int),
                name = "",
                //story = "Finalt they are here",
                //language = (int)Languages.en
            };

            enclosureController.UpdateEnclosure(enc);
        }

        [TestMethod]
        public void UpdateEnclosureValidInput()
        {
            var encs = enclosureController.GetAllEnclosures();

            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var monkeyEncEn = encs.SingleOrDefault(en => en.name == "תצוגת הקופים");
            Assert.IsNotNull(monkeyEncEn);

            monkeyEncEn.name = "קקי";
            enclosureController.UpdateEnclosure(monkeyEncEn);

            encs = enclosureController.GetAllEnclosures();
            Assert.AreEqual(4, encs.Count());

            var updatedEnc = encs.SingleOrDefault(en => en.name == "קקי");
            Assert.IsNotNull(updatedEnc);

            var nothing = encs.SingleOrDefault(en => en.name == "תצוגת הקופים");
            Assert.IsNull(nothing);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureExistingName()
        {
            var encs = enclosureController.GetAllEnclosures();
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var monkeyEncEn = encs.SingleOrDefault(en => en.name == "תצוגת הקופים");
            Assert.IsNotNull(monkeyEncEn);

            Enclosure zebra = new Enclosure
            {
                id = monkeyEncEn.id,
                markerY = monkeyEncEn.markerY,
                markerX = monkeyEncEn.markerX,
                name = "זברה"
            };

            enclosureController.UpdateEnclosure(zebra);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureEmptyName()
        {
            var encs = enclosureController.GetAllEnclosures();
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var monkeyEncEn = encs.SingleOrDefault(en => en.name == "תצוגת הקופים");
            Assert.IsNotNull(monkeyEncEn);

            Enclosure zebra = new Enclosure
            {
                id = monkeyEncEn.id,
                markerY = monkeyEncEn.markerY,
                markerX = monkeyEncEn.markerX,
                name = ""
            };

            enclosureController.UpdateEnclosure(zebra);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDoesntExists()
        {
            var encs = enclosureController.GetAllEnclosures();
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var monkeyEncEn = encs.SingleOrDefault(en => en.name == "תצוגת הקופים");
            Assert.IsNotNull(monkeyEncEn);

            Enclosure zebra = new Enclosure
            {
                id = -4,
                markerY = monkeyEncEn.markerY,
                markerX = monkeyEncEn.markerX,
                name = "זברה"
            };

            enclosureController.UpdateEnclosure(zebra);
        }
        #endregion

        #region UpdateEnclosureDetails

        [TestMethod]
        public void UpdateEnclosureDetailsAddEncDetValidInput()
        {
            var details = enclosureController.GetEnclosureDetailsById(4);
            Assert.AreEqual(1, details.Count());
            Assert.AreEqual("קרנף לבן", details.First().name);

            var enc = new EnclosureDetail
            {
                encId = 4,
                name = "Rhino enclosure",
                language = (int)Languages.en,
                story = "This is a story"
            };

            enclosureController.UpdateEnclosureDetail(enc);

            details = enclosureController.GetEnclosureDetailsById(4);
            Assert.AreEqual(2, details.Count());
            Assert.AreEqual("Rhino enclosure", details.SingleOrDefault(d => d.language == (int)Languages.en).name);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDetailsAddEncDetExistingName()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new EnclosureDetail
            {
                encId       = 4,
                name        = "Monkeys",
                story       = "Finalt they are here",
                language    = (int)Languages.en
            };

            enclosureController.UpdateEnclosureDetail(enc);
        }

        [TestMethod]
        public void UpdateEnclosureDetailsUpdateEncDetValidInput()
        {
            var details = enclosureController.GetEnclosureDetailsById(4);
            Assert.AreEqual(1, details.Count());
            Assert.AreEqual("קרנף לבן", details.First().name);

            var enc = new EnclosureDetail
            {
                encId = 2,
                language = (int)Languages.he, //hebrew
                name = "קופי אדם",
                story = "הקופים שלנו הם הכי חכמים!"

            };

            enc.name = "123";

            enclosureController.UpdateEnclosureDetail(enc);

            details = enclosureController.GetEnclosureDetailsById(2);
            Assert.AreEqual(2, details.Count());
            Assert.AreEqual("123", details.SingleOrDefault(d => d.language == (int)Languages.he).name);
            Assert.AreEqual("Houman Monkeys", details.SingleOrDefault(d => d.language == (int)Languages.en).name);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDetailsEncDoesntExists()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.AreEqual(4, encs.Count());

            var enc = new EnclosureDetail
            {
                encId = -4,
                name = "Lions enclosure",
                language = (int)Languages.en,
                story = "This is a story"
            };

            enclosureController.UpdateEnclosureDetail(enc);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDetailsEmptyName()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new EnclosureDetail
            {
                encId = 2,
                name = "",
                story = "Finalt they are here",
                language = (int)Languages.en
            };

            enclosureController.UpdateEnclosureDetail(enc);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDetailsWrongLanguage()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new EnclosureDetail
            {
                encId = 2,
                name = "Monkeys",
                story = "Finalt they are here",
                language = -4
            };

            enclosureController.UpdateEnclosureDetail(enc);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureDetailsUpdateEncDetExistingName()
        {
            var encs = enclosureController.GetAllEnclosureResults((int)Languages.en);
            Assert.IsNotNull(encs);
            Assert.AreEqual(4, encs.Count());

            var enc = new EnclosureDetail
            {
                encId = 2,
                language = (int)Languages.en, //english
                name = "Houman Monkeys",
                story = "Computer Science students."

            };

            enc.name = "Monkeys";

            enclosureController.UpdateEnclosureDetail(enc);
        }

        #endregion

        #region UpdateVideoUrl

        [TestMethod]
        public void UpdateEnclosureVideoAddEncVidValidInput()
        {
            var videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(1, videos.Count());

            var vid = videos.First();
            Assert.AreEqual("video1", vid.videoUrl);
            Assert.AreEqual(1, vid.enclosureId);

            var newVid = new YoutubeVideoUrl
            {
                id          = default(int),
                enclosureId = 1,
                videoUrl    = "video4"
            };

            enclosureController.UpdateEnclosureVideo(newVid);

            videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(2, videos.Count());
            Assert.AreEqual("video4", videos.SingleOrDefault(p => p.videoUrl== "video4").videoUrl);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureVidoeEncDoesntExists()
        {
            var videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(1, videos.Count());

            var vid = videos.First();
            Assert.AreEqual("video1", vid.videoUrl);
            Assert.AreEqual(1, vid.enclosureId);

            var newVid = new YoutubeVideoUrl
            {
                id = default(int),
                enclosureId = -1,
                videoUrl = "video4"
            };

            enclosureController.UpdateEnclosureVideo(newVid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureVideosEmptyUrl()
        {
            var videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(1, videos.Count());

            var vid = videos.First();
            Assert.AreEqual("video1", vid.videoUrl);
            Assert.AreEqual(1, vid.enclosureId);

            var newVid = new YoutubeVideoUrl
            {
                id = default(int),
                enclosureId = 1,
                videoUrl = ""
            };

            enclosureController.UpdateEnclosureVideo(newVid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEnclosureVideoAddEncVidUrlExists()
        {
            var videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(1, videos.Count());

            var vid = videos.First();
            Assert.AreEqual("video1", vid.videoUrl);
            Assert.AreEqual(1, vid.enclosureId);

            var newVid = new YoutubeVideoUrl
            {
                id = default(int),
                enclosureId = 1,
                videoUrl = "video2"
            };

            enclosureController.UpdateEnclosureVideo(newVid);
        }

        #endregion

        #region UpdateRecurringEvent

        [TestMethod]
        public void UpdateRecurringEventAddRecEventValidInput()
        {
            var events = enclosureController.GetRecurringEvents(2,2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id          = default(int),
                enclosureId = 2,
                day         = 1,
                description = "Looking",
                startTime   = new TimeSpan(10,30,00),
                endTime     = new TimeSpan(11,30,00),
                language    = (int)Languages.en
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);

            events = enclosureController.GetRecurringEvents(2,2);
            Assert.AreEqual(3, events.Count());
            Assert.IsNotNull(events.SingleOrDefault(re => re.description == "Looking"));
        }

        [TestMethod]
        public void UpdateRecurringEventAddRecEventCloseTimeValidInput()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 7,
                description = "Looking",
                startTime = new TimeSpan(10, 00, 00),
                endTime = new TimeSpan(10, 30, 00),
                language = (int)Languages.en
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);

            events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(3, events.Count());
            Assert.IsNotNull(events.SingleOrDefault(re => re.description == "Looking"));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventEmptyDesc()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 12,
                description = "",
                startTime = new TimeSpan(10, 30, 00),
                endTime = new TimeSpan(11, 30, 00),
                language = (int)Languages.en
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);

            events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(3, events.Count());
            Assert.IsNotNull(events.SingleOrDefault(re => re.description == "Looking"));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventWrongEnclosure()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = -2,
                day = 12,
                description = "Looking",
                startTime = new TimeSpan(10, 30, 00),
                endTime = new TimeSpan(11, 30, 00),
                language = (int)Languages.en
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventWrongDay()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 8,
                description = "Looking",
                startTime = new TimeSpan(10, 30, 00),
                endTime = new TimeSpan(11, 30, 00),
                language = (int)Languages.en
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventWrongTime()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 12,
                description = "Looking",
                endTime = new TimeSpan(10, 30, 00),
                startTime = new TimeSpan(11, 30, 00),
                language = 2
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventWrongTimeEndEqualsStart()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 12,
                description = "Looking",
                endTime = new TimeSpan(11, 30, 00),
                startTime = new TimeSpan(11, 30, 00),
                language = 2
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventWrongLanguage()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 12,
                description = "Looking",
                startTime = new TimeSpan(10, 30, 00),
                endTime = new TimeSpan(11, 30, 00),
                language = -1
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventAddEventTimeExists()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = default(int),
                enclosureId = 2,
                day = 17,
                description = "Looking",
                startTime = new TimeSpan(10, 30, 00),
                endTime = new TimeSpan(11, 30, 00),
                language = 2
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        public void UpdateRecurringEventUpdateRecEventValidInput()
        {
            var events = enclosureController.GetRecurringEvents(1);
            Assert.AreEqual(1, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(1, recEvent.id);
            Assert.AreEqual(1, recEvent.day);
            Assert.AreEqual("האכלה", recEvent.description);
            
            var newRecEvent = new RecurringEvent
            {
                id = 6,
                enclosureId = 2,
                day = 7,
                description = "Feeding",
                startTime = new TimeSpan(10, 30, 0),
                endTime = new TimeSpan(11, 0, 0),
                language = (int) Languages.en
            };

            newRecEvent.description = "kaki";
            newRecEvent.day = 6;

            enclosureController.UpdateRecurringEvent(newRecEvent);

            events = enclosureController.GetRecurringEvents(2,2);
            Assert.AreEqual(2, events.Count());
            Assert.IsNotNull(events.SingleOrDefault(re => re.description == "kaki"));
            Assert.IsNotNull(events.SingleOrDefault(re => re.day == 6));
            Assert.IsNull(events.SingleOrDefault(re => re.day == 17));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventUpdateEventDoesntExists()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = -1,
                enclosureId = 1,
                day = 1,
                description = "האכלה",
                startTime = new TimeSpan(10, 30, 0),
                endTime = new TimeSpan(11, 0, 0),
                language = (int)Languages.he
            };

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateRecurringEventUpdateTimeExists()
        {
            var events = enclosureController.GetRecurringEvents(2, 2);
            Assert.AreEqual(2, events.Count());

            var recEvent = events.First();
            Assert.AreEqual(4, recEvent.id);
            Assert.AreEqual(11, recEvent.day);
            Assert.AreEqual("Playing", recEvent.description);

            var newRecEvent = new RecurringEvent
            {
                id = 6,
                enclosureId = 2,
                day = 17,
                description = "Feeding",
                startTime = new TimeSpan(10, 30, 0),
                endTime = new TimeSpan(11, 0, 0),
                language = (int)Languages.en
            };

            newRecEvent.day = 11;
            newRecEvent.startTime = new TimeSpan(13, 15, 0);
            newRecEvent.endTime = new TimeSpan(13, 45, 0);

            enclosureController.UpdateRecurringEvent(newRecEvent);
        }

        #endregion

        #region DeleteEnclosure

        [TestMethod]
        public void DeleteEnclosureValidInput()
        {
            var encs = enclosureController.GetAllEnclosures();
            var details = enclosureController.GetEnclosureDetailsById(4);
            var animalsController = new AnimalController();

            Assert.AreEqual(0, enclosureController.GetRecurringEvents(4, (int)Languages.he).Count());
            Assert.AreEqual(0, animalsController.GetAnimalResultByEnclosure(4, (int)Languages.he).Count());
            Assert.AreEqual(4, encs.Count());
            Assert.AreEqual(1, details.Count());

            enclosureController.DeleteEnclosure(4);

            encs = enclosureController.GetAllEnclosures();
            Assert.AreEqual(3, encs.Count());

            details = enclosureController.GetEnclosureDetailsById(4);
            Assert.AreEqual(0, details.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEnclosureExistingAnimals()
        {
            var encs = enclosureController.GetAllEnclosures();
            var details = enclosureController.GetEnclosureDetailsById(1);
            var animalsController = new AnimalController();

            var recurringEvents = enclosureController.GetRecurringEvents(1, 1);
            Assert.AreEqual(1, recurringEvents.Count());
            enclosureController.DeleteRecurringEvent(1, (int)recurringEvents.First().id);
            Assert.AreEqual(0, enclosureController.GetRecurringEvents(1, 1).Count());

            Assert.AreEqual(2, animalsController.GetAnimalResultByEnclosure(1, (int)Languages.he).Count());
            Assert.AreEqual(4, encs.Count());
            Assert.AreEqual(2, details.Count());

            enclosureController.DeleteEnclosure(1);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEnclosureExistingRecurringEvents()
        {
            var encs = enclosureController.GetAllEnclosures();
            var details = enclosureController.GetEnclosureDetailsById(1);
            var animalsController = new AnimalController();

            var animals = animalsController.GetAnimalResultByEnclosure(1, (int)Languages.he).ToList();
            Assert.AreEqual(2, animals.Count());
            foreach (AnimalResult a in animals)
            {
                animalsController.DeleteAnimal((int)a.Id);
            }
            animals = animalsController.GetAnimalResultByEnclosure(1, (int)Languages.he).ToList();
            Assert.AreEqual(0, animals.Count());

            Assert.AreEqual(4, encs.Count());
            Assert.AreEqual(2, details.Count());

            Assert.AreEqual(1, enclosureController.GetRecurringEvents(1, 1).Count());

            enclosureController.DeleteEnclosure(1);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEnclosureWrongId()
        {
            enclosureController.DeleteEnclosure(-1);
        }

        #endregion

        #region DeleteEnclosurePicture

        [TestMethod]
        public void DeleteEnclosurePictureValidInput()
        {
            var pictures = enclosureController.GetEnclosurePicturesById(2);

            Assert.AreEqual(2, pictures.Count());

            var pic = pictures.First();
            Assert.AreEqual("url2", pic.pictureUrl);

            enclosureController.DeleteEnclosurePicture(pic.enclosureId, (int)pic.id);

            pictures = enclosureController.GetEnclosurePicturesById(2);
            Assert.AreEqual(1, pictures.Count());

            pic = pictures.First();
            Assert.AreEqual("url3", pic.pictureUrl);

            enclosureController.DeleteEnclosurePicture(pic.enclosureId, (int)pic.id);

            pictures = enclosureController.GetEnclosurePicturesById(2);
            Assert.AreEqual(0, pictures.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEnclosurePictureWrongId()
        {
            enclosureController.DeleteEnclosurePicture(-1, -1);
        }
        #endregion

        #region DeleteEnclosureVideo

        [TestMethod]
        public void DeleteEnclosureVideoValidInput()
        {
            var videos = enclosureController.GetEnclosureVideosById(1);

            Assert.AreEqual(1, videos.Count());

            var vid = videos.First();
            Assert.AreEqual("video1", vid.videoUrl);

            enclosureController.DeleteEnclosureVideo((int)vid.enclosureId, (int)vid.id);

            videos = enclosureController.GetEnclosureVideosById(1);
            Assert.AreEqual(0, videos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEnclosureVideoWrongId()
        {
            enclosureController.DeleteEnclosurePicture(-1, -1);
        }

        #endregion

        #region DeleteRecurringEvent
        [TestMethod]
        public void DeleteRecurringEventValidInput()
        {
            var enc = enclosureController.GetAllEnclosures().SingleOrDefault(e => e.name == "קופי אדם");
            Assert.IsNotNull(enc);

            var allRecEve = enclosureController.GetRecurringEvents((int)enc.id);
            Assert.AreEqual(2, allRecEve.Count());

            var eve = allRecEve.SingleOrDefault(e => e.id == 3);
            Assert.AreEqual(eve.day, 1);
            Assert.AreEqual(eve.description, "משחק");

            enclosureController.DeleteRecurringEvent(eve.enclosureId, (int)eve.id);

            allRecEve = enclosureController.GetRecurringEvents((int)enc.id);
            Assert.AreEqual(1, allRecEve.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteRecuringEventIdDoesntExists()
        {
            enclosureController.DeleteRecurringEvent(1, 1000);
        }
        #endregion
    }
}